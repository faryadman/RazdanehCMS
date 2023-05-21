using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Project.Application.DTOs.Datatable.Base;
using System.Linq.Expressions;

namespace Project.Application.Extensions
{
    public static class DatatableExtention
    {
        public static void GetDataFromRequest(this HttpRequest Request, out FiltersFromRequestDataTable filtersFromRequest)
        {
            //TODO: Make Strings Safe String
            filtersFromRequest = new FiltersFromRequestDataTable
            {
                Draw = Request.Form["draw"].FirstOrDefault(),
                Start = Convert.ToInt32(Request.Form["start"].FirstOrDefault()),
                Length = Convert.ToInt32(Request.Form["length"].FirstOrDefault()),
                SortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault(),
                SortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault(),
                SearchValue = Request.Form["search[value]"].FirstOrDefault().NormalizeText()
            };
            filtersFromRequest.SortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();

            filtersFromRequest.SearchValue = filtersFromRequest.SearchValue?.ToLower();

        }

        public static async Task<DatatableResponse<TDest>> ToDataTableAsync<T, TDest>(this IQueryable<T> source, int totalRecords, FiltersFromRequestDataTable filtersFromRequest, IMapper _mapper)
        {
            var res = new DatatableResponse<TDest>
            {
                SEcho = filtersFromRequest.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = await source.CountAsync(),
            };

            List<T> data;

            if (string.IsNullOrWhiteSpace(filtersFromRequest.SortColumn) == false)
            {
                data = await source
                    .OrderByDynamic("UpdatedAt", "DESC")
                    //.DatatableOrderBy(filtersFromRequest)
                    .DatatablePaginate(filtersFromRequest.Start, filtersFromRequest.Length)
                    .ToListAsync();
            }
            else
            {
                data = await source
                    .OrderByDynamic("UpdatedAt", "DESC")
                    //.DatatableOrderBy(filtersFromRequest)
                    .DatatablePaginate(filtersFromRequest.Start, filtersFromRequest.Length)
                    .ToListAsync();
            }

            res.Data = _mapper.Map<List<TDest>>(data);

            return res;
        }

        private static IQueryable<T> DatatableOrderBy<T>(this IQueryable<T> source, FiltersFromRequestDataTable filtersFromRequest)
        {
            var props = typeof(T).GetProperties();
            string propertyName = "";
            for (int i = 0; i < props.Length; i++)
            {
                if (i.ToString() == filtersFromRequest.SortColumnIndex)
                    propertyName = props[i].Name;
            }
            System.Reflection.PropertyInfo propByName = typeof(T).GetProperty(propertyName);
            if (propByName != null)
            {
                var param = Expression.Parameter(typeof(T));
                var expr = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(Expression.Property(param, propByName), typeof(object)),
                    param
                );
                if (filtersFromRequest.SortColumnDirection == "desc")
                    source = source.OrderByDescending(expr);
                else
                    source = source.OrderBy(expr);
            }

            return source;
        }

        public static IQueryable<T> DatatablePaginate<T>(this IQueryable<T> queryable, int start, int length)
        {
            return queryable.Skip(start).Take(length);
        }

    }
}