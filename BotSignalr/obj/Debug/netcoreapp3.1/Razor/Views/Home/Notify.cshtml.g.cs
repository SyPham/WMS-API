#pragma checksum "F:\0.Document\angular9\WMS-API\BotSignalr\Views\Home\Notify.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "aa830db2256fc5b3960caf6d6c52fbc8ede8fcef"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Notify), @"mvc.1.0.view", @"/Views/Home/Notify.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "F:\0.Document\angular9\WMS-API\BotSignalr\Views\_ViewImports.cshtml"
using BotSignalr;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "F:\0.Document\angular9\WMS-API\BotSignalr\Views\_ViewImports.cshtml"
using BotSignalr.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"aa830db2256fc5b3960caf6d6c52fbc8ede8fcef", @"/Views/Home/Notify.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e89610745160ac31037260019046ef54c8d7a053", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_Notify : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/lib/jquery/dist/jquery.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 2 "F:\0.Document\angular9\WMS-API\BotSignalr\Views\Home\Notify.cshtml"
  
    ViewData["Title"] = "Notify";
    Layout = "~/Views/Shared/_Layout.cshtml";
    var code = ViewBag.Code as string;

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n<h1>Notify</h1>\r\n<button class=\"btn btn-primary\" id=\"Button1\">\r\n    Get Token\r\n</button>\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "aa830db2256fc5b3960caf6d6c52fbc8ede8fcef3831", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n<script>\r\n    function ExecuteAPI() {\r\n        var formData = new FormData();\r\n        formData.append(\'grant_type\', \'authorization_code\');\r\n        formData.append(\'code\', \'");
#nullable restore
#line 17 "F:\0.Document\angular9\WMS-API\BotSignalr\Views\Home\Notify.cshtml"
                            Write(code);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"');
        formData.append('client_id', 'HF6qOCM9xL4lXFsqOLPzhJ');
        formData.append('redirect_uri', 'http://localhost:61621/Home/Notify');
        formData.append('client_secret', 'IvjiGAE8TAD8DOONBJ0Z71Ir9daUNlqMsy69ebokcQN');

        $.ajax({
            url: ""https://notify-bot.line.me/oauth/token"",
            type: ""post"",
            crossDomain: true,
            contentType: ""application/x-www-form-urlencoded"",
            success: function (apiResult) {
                console.log(apiResult)
            },
            error: function (ex) {
                console.log(ex)
            }
        })
    }
        //hook event
        $(document).ready(function () {
            $('#Button1').click(ExecuteAPI);
        });

</script>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
