using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace SwaggerUitl
{
    /// <summary>
    /// swgger服务拓展
    /// </summary>
    public static class SwaggerServiceExtensions
    {
        /// <summary>
        /// 注册swagger服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerCustom(this IServiceCollection services, IConfiguration configuration, string projectName = "", string version = "v1",Action<SwaggerGenOptions> registAction = null,string modelProjectName="")
        {
            //注册SwaggerAPI文档服务
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                options.AddSecurityRequirement(security);

                options.SwaggerDoc(projectName, new Info
                {
                    Title = projectName == "" ? AppDomain.CurrentDomain.FriendlyName : projectName,
                    Version = "v1",
                });
                //使用过滤器单独对某些API接口实施认证
                options.OperationFilter<SwaggerOperationFilter>();
                options.OperationFilter<AddFileParamTypesOperationFilter>();

                if (registAction != null)
                {
                    registAction(options);
                }

                //获取应用程序根目录路径，官方写法
                var basePath = AppContext.BaseDirectory;
                var xmlName = projectName == "" ? AppDomain.CurrentDomain.FriendlyName : projectName;
                var xmlPath = Path.Combine(basePath, $"{xmlName}.xml");
                if (System.IO.File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);

                if (!string.IsNullOrEmpty(modelProjectName))
                {
                    var modelXmlPath = Path.Combine(basePath, $"{modelProjectName}.xml");
                    if (System.IO.File.Exists(modelXmlPath))
                        options.IncludeXmlComments(modelXmlPath);
                }
            });

            return services;
        }

        /// <summary>
        /// swagger配置，必须在app.UseMvc()之前调用
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerCustom(this IApplicationBuilder builder, IConfiguration configuration, string projectName = "", string version = "v1")
        {
            //启用Swagger
            builder.UseSwagger(c=> {
                c.RouteTemplate = "{documentName}/swagger.json";
            });
            //启用SwaggerUI
            builder.UseSwaggerUI(options =>
            {
                var xmlName = projectName == "" ? AppDomain.CurrentDomain.FriendlyName : projectName;
                //文档终结点
                options.SwaggerEndpoint($"/{projectName}/swagger.json", $"API {version}");//{projectName}
                //文档标题
                options.DocumentTitle = projectName == "" ? AppDomain.CurrentDomain.FriendlyName : projectName;
                //页面API文档格式 Full=全部展开， List=只展开列表, None=都不展开
                options.DocExpansion(DocExpansion.List);

                options.RoutePrefix = projectName;

                //options.IndexStream = () => Assembly.GetExecutingAssembly().GetManifestResourceStream("PolicingAlertCenter.wwwroot.swagger.ui.index.html");
            });
            return builder;
        }
    }
}
