using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Ultilities.Helpers
{
    public static class SwaggerExtension
    {
        public static void UseCustomSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Optima API v1");
                c.DisplayRequestDuration();
                c.DefaultModelsExpandDepth(-1);
            });
        }
    }
}
