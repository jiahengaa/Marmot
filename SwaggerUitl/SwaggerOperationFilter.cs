using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SwaggerUitl
{
    /// <summary>
    /// swagger操作过滤器
    /// </summary>
    public class SwaggerOperationFilter : IOperationFilter
    {
        /// <summary>
        /// 仅标注有AuthorizeAttribute，需要授权的api，在头部添加token
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Swashbuckle.AspNetCore.Swagger.Operation operation, OperationFilterContext context)
        {
            operation.Parameters = operation.Parameters ?? new List<IParameter>();
            var info = context.MethodInfo;
            context.ApiDescription.TryGetMethodInfo(out info);
            try
            {
                Attribute attribute = info.GetCustomAttribute(typeof(AuthorizeAttribute));
                if (attribute != null)
                {
                    operation.Parameters.Add(new BodyParameter
                    {
                        Name = "Authorization",
                        @In = "header",
                        Description = "access_token",
                        Required = true
                    });
                }

            }
            catch
            { }
        }

    }
}
