# AdminPortal
The solution is build using ASP.NET Core using .Net Framework 4.6.1  

Currently required Visual Studio 2015 including [NET Core tools Preview 2 for Visual Studio 2015](https://go.microsoft.com/fwlink/?LinkID=827546).
The code is not converted to VS 2017 yet.

To be able run locally and login using AAD you need to use registered in AAD port 
In  Source\src\AdminPortal\Properties\launchSettings.json set
  "iisSettings": {    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:50903/",
      "sslPort": 44396
    }
  },
