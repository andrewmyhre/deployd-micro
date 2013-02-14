﻿using Nancy;
using Nancy.TinyIoc;

namespace deployd.mothership.AppStart
{
    public class NancyConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            Conventions.ViewLocationConventions.Add((viewName, model, context) => string.Concat("Views/", context.ModuleName.Replace("Controller",""), "/", viewName));
        }
    }
}
