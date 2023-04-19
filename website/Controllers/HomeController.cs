using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using website.Models;
using website.DBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;

namespace website.Controllers;

public class BaseController : Controller
{
    public LineUser? _User { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        _User = (LineUser)HttpContext.Items["User"];
    }
}

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly EFCoreContext _context;
    private readonly IConfiguration _config;
    
    

    public HomeController(ILogger<HomeController> logger, EFCoreContext context, IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _config = config;

    }
    public IActionResult Index()
    {
        _logger.LogInformation("Hello, world!");
        if(_User != null)
        {
            
            if(_User.registed == true)
            {
                ViewData["Message"] = "Hello " + _User.Name +"，你看起來已經註冊我們的LineNotify了！如果要取消，請按下下面按鈕。";
                ViewData["ButtonLabel"] = "取消我的AccessToken";
            }else
            {
                ViewData["Message"] = "Hello " + _User.Name +"，你看起來還沒有註冊我們的LineNotify了！如果要註冊，請按下下面按鈕";
                ViewData["ButtonLabel"] = "我要註冊~";
            }
        }
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> ProcessData()
    {
        // 將 inputText 參數的值存儲到 ViewData 中
        // ViewData["Message"] = $"You entered: {inputText}";
        

        // return View("Index");
        if(_User.registed == true)//測消掉accesstoken
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://notify-api.line.me/api/revoke");
            request.Headers.Add("Authorization", "Bearer "+_User.AccessToken);
            _logger.LogInformation("希望撤銷的AccessToken:"+_User.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation(await response.Content.ReadAsStringAsync());

            _User.AccessToken = "";
            _User.registed = false;
            _User =  _context.UpdateLineUserAsync(_User).Result;
            ViewData["Message"] = $"已撤銷成功~如果通知請再重新註冊";
            return View("Index");
        }
        else//如果沒註冊，按下按鈕就是要去註冊
        {
            return Redirect("https://notify-bot.line.me/oauth/authorize?response_type=code&client_id="+_config["LineNotify:ClientId"]+"&state=123123&scope=notify&redirect_uri=https://samnchiulinechatbot.azurewebsites.net/Home/Back");
        }
    }

    public async Task<IActionResult> Back(string code, string state)
    {
         using var httpClient = new HttpClient();

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", "https://samnchiulinechatbot.azurewebsites.net/Home/Back"),
                new KeyValuePair<string, string>("client_id", _config["LineNotify:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _config["LineNotify:ClientSecret"])
            });

            using var response = await httpClient.PostAsync("https://notify-bot.line.me/oauth/token", requestBody);

            //response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);
            // 範例回傳內容：{"status" : 200, "message" : "access_token is issued", "access_token" : "RE1pjwHPJCjRZ4HG3yEoCyzmaF6mY4OYD3O4EOaOMCq"}

            // 解析 JSON 格式的回傳內容，取得 access_token
            var json = System.Text.Json.JsonDocument.Parse(responseBody);
            var accessToken = json.RootElement.GetProperty("access_token").GetString();
            if (_User != null)
                {
                    _User.AccessToken = accessToken;
                    _User.registed = true;
                    _User =  _context.UpdateLineUserAsync(_User).Result;
                    
                }
            
            ViewData["Message"] = accessToken;
        return View();
    }

    public async Task<IActionResult> Manage()
    {
        //ViewData["Manage"] = $"You entered: {inputText}";
        var sendLogs = await _context.GetAllSendLogsAsync();
        ViewData["SendLogs"] = sendLogs;
        return View();
    }

    public async Task<IActionResult> Logout()
    {

        if(_User == null)
        return RedirectToAction(nameof(Index));

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://notify-api.line.me/api/revoke");
        request.Headers.Add("Authorization", "Bearer "+_User.AccessToken);
        _logger.LogInformation("希望撤銷的AccessToken:"+_User.AccessToken);
            
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        _logger.LogInformation(await response.Content.ReadAsStringAsync());

        _User.AccessToken = "";
        _User.registed = false;
        _User =  _context.UpdateLineUserAsync(_User).Result;
        // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        
        foreach (var cookieKey in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookieKey);
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SendAll(string inputText)
    {
        

        var users = await _context.GetAllLineUsersAsync();
        if(users != null)

        foreach(LineUser user in users)
        {
            if(user.registed == false)
                continue;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _config["LineNotify:notify"]);
            request.Headers.Add("Authorization", "Bearer " +  user.AccessToken);
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new("message",inputText ));
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string status = await  response.Content.ReadAsStringAsync();
            Console.WriteLine(status);

            SendLog sendLog = new SendLog();
            sendLog.sender = _User.Name;
            sendLog.receiver = user.Name;
            sendLog.message = inputText;
            sendLog.status = status;
            await _context.AddSendLogAsync(sendLog);
        }

        ViewData["Manage"] = $"You send: {inputText}";
        var sendLogs = await _context.GetAllSendLogsAsync();
        ViewData["SendLogs"] = sendLogs;
        return View("Manage");
    }
    // public async Task Logout()
    // {
    //     _User.AccessToken = "";
    //     _User.registed = false;
    //     await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    //     await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    // }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    
}
