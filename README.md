# Introduction 
這是 **20230415 精通 OAuth 2.0 授權框架** 的回家作業

# 實作包含
這次的專案實作包含了
1.	網站本體使用LineLogin來進行OAuth2.0的實作，認證後將自動註冊成會員
2.	首頁提供LineNotify的功能連動，透過OAuth2.0的授權後，會將使用者的AccessToken儲存在網站中，並提供撤銷功能
3.	網站本身沒提供後台(太忙了，綁起來就都傪在一起做灑尿牛丸了）而是使用者註冊後，都可以群發
1.	登出後，AccessToken會自動從資料庫中刪除。

Demo網站連結：https://samnchiulinechatbot.azurewebsites.net/


# 這次踩到的雷
1.	因為自己大多都寫API為主，MVC好久沒寫了，所以DI與MiddleWare都快忘光了，甚至連Controller 與View怎樣互動都忘了，還好有ChatGPT。
1.	Programs.cs前面對於整個網站LineLogin的Oidc Dependency Injection (DI)已經把/CallBack用掉了，所以在寫LineNotify的callback的時候卡了一下，不過還好保哥的課有聽懂，所以自己再實作出另外一個。
1.	Azure AppService的不熟悉，所以想要導入Serilog來協助我把Log吐出來除錯，但這次ChatGPT一直不斷跟我鬼打牆，最後靠了保哥的 [.NET 6.0 如何使用 Serilog 對應用程式事件進行結構化紀錄](https://blog.miniasp.com/post/2021/11/29/How-to-use-Serilog-with-NET-6)來解決，ChatGPT抄了一堆程式從這來，但這次沒成功幫我解決問題。
1.	Azure DevOps release到Azure AppService，該死的run for package模式，讓我連Log都產不出來，鬼打牆一陣子後，終於搞定，這次就是ChatGPT協助我完成的了。

# 最後感想
1.	保哥說的話，要聽，不然就去找保哥的文章。
1.	ChatGPT 刷下去，對於生產力不足的我有顯著的上升