
using Application.Core.SignalR;

public static class RegisterStartupMiddlewares
    {
        public static WebApplication SetupMiddleware(this WebApplication app)
        {
        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Application.Api v1"));
            //comment out when publish.
            // app.UseSwaggerUI(c => c.SwaggerEndpoint("/pms/swagger/v1/swagger.json", "Application.Api v1"));
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = ctx => {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
                  "Origin, X-Requested-With, Content-Type, Accept");
            },

        });
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<BroadcastHub>("/notify");

        return app;
        }
    }

