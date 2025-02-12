using AccessDemo.Common.Contracts;
using AccessDemo.Common.Models;
using DbAccess.Models;
using Microsoft.AspNetCore.Mvc;
using DbAccess.Contracts;
using System.Reflection;

namespace AccessDemo.Api;


/// <summary>
/// Api Endpoint extension for DbAccess services
/// </summary>
public static class EndpointExtension
{
    /// <summary>
    /// MapDbAccessEndpoints
    /// </summary>
    /// <param name="app">WebApplication</param>
    /// <returns></returns>
    public static WebApplication MapDbAccessEndpoints(this WebApplication app)
    {
        app.MapDefaults<IProviderService, Provider>();
        app.MapDefaultsExt<IAreaService, Area, ExtArea>();
        app.MapDefaultsExt<IPackageService, Package, ExtPackage>(mapSearch: true);
        app.MapDefaultsExt<IResourceService, Resource, ExtResource>();
        app.MapCrossDefaults<IPackageResourceService, PackageResource, ExtPackageResource, Package, Resource>("packages","resources");

        return app;
    }

    /// <summary>
    /// MapDefaults
    /// </summary>
    /// <typeparam name="TRepo"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="app">WebApplication</param>
    /// <param name="mapPost">mapPost</param>
    /// <param name="mapPut">mapPut</param>
    /// <param name="mapDelete">mapDelete</param>
    /// <param name="mapGetAll">mapGetAll</param>
    /// <param name="mapIngest">mapIngest</param>
    /// <param name="mapSearch">mapSearch</param>
    /// <returns></returns>
    public static WebApplication MapDefaults<TRepo, T>(this WebApplication app, bool mapPost = true, bool mapPut = true, bool mapDelete = true, bool mapGetAll = true, bool mapIngest = false, bool mapSearch = false)
        where TRepo : IDbBasicRepository<T>
    {
        string name = typeof(T).Name;
        app.MapGet($"/{name.ToLower()}" + "/{id}", (HttpRequest request, [FromServices] TRepo service, Guid id) => { return service.Get(id, options: GenerateRequestOptions(request)); }).WithOpenApi().WithTags(name).WithSummary("Get single " + name);
        if (mapSearch)
        {
            //app.MapGet($"/{name.ToLower()}" + "/search/{term}", (HttpRequest request, TRepo service, string term) => { return service.Search(term, options: GenerateRequestOptions(request)); }).WithOpenApi().WithTags(name).WithSummary("Search " + name);
        }

        if (mapGetAll)
        {
            app.MapGet($"/{name.ToLower()}", (HttpRequest request, [FromServices] TRepo service) => { return service.Get(options: GenerateRequestOptions(request)); }).WithOpenApi().WithTags(name).WithSummary("Get all " + name);
        }

        if (mapPost)
        {
            app.MapPost($"/{name.ToLower()}", async ([FromServices] TRepo service, [FromBody] T obj) =>
            {
                var res = await service.Create(obj);
                return res > 0 ? Results.Created<T>("/", obj) : Results.Problem();
            }).WithOpenApi().WithTags(name).WithSummary("Create " + name);
        }

        if (mapIngest)
        {
            app.MapPost($"/{name.ToLower()}/ingest", async ([FromServices] TRepo service, [FromBody] List<T> data) =>
            {
                var res = await service.Ingest(data);
                return res > 0 ? Results.CreatedAtRoute<T>() : Results.Problem();
            }).WithOpenApi().WithTags(name).WithSummary("Ingest list of " + name);
        }

        if (mapPut)
        {
            app.MapPut($"/{name.ToLower()}" + "/{id}", ([FromServices] TRepo service, Guid id, [FromBody] T obj) =>
            {
                return service.Update(id, obj);
            }).WithOpenApi().WithTags(name).WithSummary("Update " + name);
        }

        if (mapDelete)
        {
            app.MapDelete($"/{name.ToLower()}" + "/{id}", ([FromServices] TRepo service, Guid id) =>
            {
                return service.Delete(id);
            }).WithOpenApi().WithTags(name).WithSummary("Delete " + name);
        }

        return app;
    }

    /// <summary>
    /// MapDefaultsExt
    /// </summary>
    /// <typeparam name="TRepo"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TExtended"></typeparam>
    /// <param name="app">WebApplication</param>
    /// <param name="mapPost">mapPost</param>
    /// <param name="mapPut">mapPut</param>
    /// <param name="mapDelete">mapDelete</param>
    /// <param name="mapGetAll">mapGetAll</param>
    /// <param name="mapIngest">mapIngest</param>
    /// <param name="mapSearch">mapSearch</param>
    /// <returns></returns>
    public static WebApplication MapDefaultsExt<TRepo, T, TExtended>(this WebApplication app, bool mapPost = true, bool mapPut = true, bool mapDelete = true, bool mapGetAll = true, bool mapIngest = false, bool mapSearch = false)
            where TRepo : IDbExtendedRepository<T, TExtended>
    {
        string name = typeof(T).Name;
        app.MapGet($"/{name.ToLower()}" + "/{id}", (HttpRequest request, [FromServices] TRepo service, Guid id) =>
        {
            return service.GetExtended(id, options: GenerateRequestOptions(request));
        }).WithOpenApi().WithTags(name).WithSummary("Get single " + name);

        if (mapSearch)
        {
            //app.MapGet($"/{name.ToLower()}" + "/search/{term}", (HttpRequest request, TRepo service, string term) =>
            //{
            //    return service.SearchExtended(term, options: GenerateRequestOptions(request));
            //}).WithOpenApi().WithTags(name).WithSummary("Search " + name);
        }

        if (mapGetAll)
        {
            app.MapGet($"/{name.ToLower()}", (HttpRequest request, [FromServices] TRepo service) =>
            {
                return service.GetExtended(options: GenerateRequestOptions(request));
            }).WithOpenApi().WithTags(name).WithSummary("Get all " + name);
        }

        if (mapPost)
        {
            app.MapPost($"/{name.ToLower()}", async ([FromServices] TRepo service, [FromBody] T obj) =>
            {
                var res = await service.Create(obj);
                return res > 0 ? Results.Created<T>("/", obj) : Results.Problem();
            }).WithOpenApi().WithTags(name).WithSummary("Create " + name);
        }

        if (mapIngest)
        {
            app.MapPost($"/{name.ToLower()}/ingest", async ([FromServices] TRepo service, [FromBody] List<T> data) =>
            {
                var res = await service.Ingest(data);
                return res > 0 ? Results.CreatedAtRoute<T>() : Results.Problem();
            }).WithOpenApi().WithTags(name).WithSummary("Ingest list of " + name);
        }

        if (mapPut)
        {
            app.MapPut($"/{name.ToLower()}" + "/{id}", ([FromServices] TRepo service, Guid id, [FromBody] T obj) =>
            {
                return service.Update(id, obj);
            }).WithOpenApi().WithTags(name).WithSummary("Update " + name);
        }

        if (mapDelete)
        {
            app.MapDelete($"/{name.ToLower()}" + "/{id}", ([FromServices] TRepo service, Guid id) =>
            {
                return service.Delete(id);
            }).WithOpenApi().WithTags(name).WithSummary("Delete " + name);
        }
        return app;
    }

    /// <summary>
    /// MapCrossDefaults
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    /// <typeparam name="TRepo"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TB"></typeparam>
    /// <param name="app">WebApplication</param>
    /// <param name="PluralNameForA">PluralNameForA</param>
    /// <param name="PluralNameForB">PluralNameForB</param>
    /// <returns></returns>
    public static WebApplication MapCrossDefaults<TRepo, T, TExtended, TA, TB>(this WebApplication app, string PluralNameForA, string PluralNameForB)
            where TRepo : IDbCrossRepository<T, TExtended, TA, TB>
    {
        var nameForT = typeof(T).Name; // PackageTag
        var nameForA = typeof(TA).Name; // Package
        var nameForB = typeof(TB).Name; // Tag

        app.MapGet($"/{nameForA.ToLower()}" + "/{id}/" + PluralNameForB, (HttpRequest request, [FromServices] TRepo service, Guid id) =>
        {
            return service.GetB(id);
        }).WithOpenApi().WithTags(nameForA).WithSummary($"Get {nameForB} based on {typeof(T).Name} filtering on {nameForA}");

        app.MapGet($"/{nameForB.ToLower()}" + "/{id}/" + PluralNameForA, (HttpRequest request, [FromServices] TRepo service, Guid id) =>
        {
            return service.GetA(id);
        }).WithOpenApi().WithTags(nameForB).WithSummary($"Get {nameForA} based on {typeof(T).Name} filtering on {nameForB}");

        return app;
    }

    private static RequestOptions GenerateRequestOptions(HttpRequest request)
    {
        var options = new RequestOptions();
        if (request == null || request.Headers == null)
        {
            return options;
        }

        if (request.Headers.AcceptLanguage.Any())
        {
            // Example from browser: nb-NO,
            // nb;q=0.9,
            // no;q=0.8,
            // nn;q=0.7,
            // en-US;q=0.6,
            // en;q=0.5
            foreach (var lang in request.Headers.AcceptLanguage)
            {
                if (string.IsNullOrEmpty(lang))
                {
                    continue;
                }

                var languageCode = lang.Split(',')[0] ?? "nb-NO";
                if (!languageCode.StartsWith("nb"))
                {
                    options.Language = languageCode;
                }
            }
        }

        if (request.Headers.ContainsKey("x-as-of-time"))
        {
            options.AsOf = DateTime.Parse(request.Headers["x-as-of-time"].ToString());
        }

        if (request.Query != null)
        {
            if (request.Query.ContainsKey("page") | request.Query.ContainsKey("page-size"))
            {
                options.UsePaging = true;
                options.PageNumber = int.Parse(request.Query["page"][0] ?? "1");
                options.PageSize = int.Parse(request.Query["page-size"][0] ?? "25");
                options.OrderBy = "name";
            }

            if (request.Query.ContainsKey("orderby"))
            {
                options.OrderBy = request.Query["orderby"][0] ?? "name";
            }
        }

        if (request.Headers != null)
        {
            if (request.Headers.ContainsKey("x-page") || request.Headers.ContainsKey("x-page-size"))
            {
                options.UsePaging = true;
                options.PageNumber = int.Parse(request.Headers["x-page"][0] ?? "1");
                options.PageSize = int.Parse(request.Headers["x-page-size"][0] ?? "25");
                options.OrderBy = "name";
            }

            if (request.Headers.ContainsKey("x-orderby"))
            {
                options.OrderBy = request.Headers["x-orderby"][0] ?? "name";
            }
        }

        return options;
    }
}

