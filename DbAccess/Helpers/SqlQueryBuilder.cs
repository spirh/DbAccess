using System.Collections.Generic;
using System.Text;
using DbAccess.Models;
using Npgsql;

namespace DbAccess.Helpers
{
    /// <summary>
    /// Responsible for building SQL queries based on the DbDefinition, RequestOptions, and filters.
    /// </summary>
    public class SqlQueryBuilder(DbDefinition definition)
    {
        private readonly DbDefinition _definition = definition;

        /*Read*/

        /// <summary>
        /// Builds a SELECT query for basic types
        /// </summary>
        public string BuildBasicSelectQuery(RequestOptions options, IEnumerable<GenericFilter> filters)
        {
            var sb = new StringBuilder();

            if (options.AsOf.HasValue)
            {
                sb.AppendLine($"set local x.asof = '{options.AsOf.Value.ToUniversalTime()}';");
            }

            sb.AppendLine("SELECT ");
            sb.AppendLine(GenerateColumns(options));
            sb.AppendLine("FROM " + GenerateSource(options));
            sb.AppendLine(GenerateFilterStatement(_definition.BaseType.Name, filters));

            string query = sb.ToString();
            return AddPagingToQuery(query, options);
        }

        /// <summary>
        /// Builds a SELECT query for extended types
        /// </summary>
        public string BuildExtendedSelectQuery(RequestOptions options, IEnumerable<GenericFilter> filters)
        {
            var sb = new StringBuilder();

            // Apply session settings (e.g., as-of)
            if (options.AsOf.HasValue)
            {
                sb.AppendLine($"set local x.asof = '{options.AsOf.Value.ToUniversalTime()}';");
            }

            sb.AppendLine("SELECT ");
            sb.AppendLine(GenerateColumns(options));

            foreach (var relation in _definition.ForeignKeys)
            {
                sb.Append(',');
                sb.AppendLine(GenerateJoinPostgresColumns(relation, options));
            }

            sb.AppendLine("FROM " + GenerateSource(options));
            foreach (var j in _definition.ForeignKeys.Where(t => !t.IsList))
            {
                var joinStatement = GetJoinPostgresStatement(j, options);
                sb.AppendLine(joinStatement);
            }

            sb.AppendLine(GenerateFilterStatement(_definition.BaseType.Name, filters));

            string query = AddPagingToQuery(sb.ToString(), options);
            Console.WriteLine(query);

            return query;
        }

        /*Write*/

        /// <summary>
        /// Builds a INSERT query
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="forTranslation"></param>
        /// <returns></returns>
        public string BuildInsertQuery(List<NpgsqlParameter> parameters, bool forTranslation = false)
        {
            return $"INSERT INTO {GetPostgresDefinition(includeAlias: false, useTranslation: forTranslation)} ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)})";
        }

        /// <summary>
        /// Builds a UPDATE query
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="forTranslation"></param>
        /// <returns></returns>
        public string BuildUpdateQuery(List<NpgsqlParameter> parameters, bool forTranslation = false)
        {
            return $"UPDATE {GetPostgresDefinition(includeAlias: false, useTranslation: forTranslation)} SET {UpdateSetStatement(parameters)} WHERE id = @_id{(forTranslation ? " AND language = @_language" : "")}";
        }

        /// <summary>
        /// Builds a UPSERT query
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string BuildUpsertQuery(List<NpgsqlParameter> parameters)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"INSERT INTO {GetPostgresDefinition(includeAlias: false)} ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)})");
            sb.AppendLine(" ON CONFLICT (id) DO ");
            sb.AppendLine($"UPDATE SET {UpdateSetStatement(parameters)}");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a DELETE query
        /// </summary>
        /// <returns></returns>
        public string BuildDeleteQuery()
        {
            return $"DELETE FROM {GetPostgresDefinition(includeAlias: false)} WHERE id = @_id";
        }

        /*Helpers*/
        private string GetPostgresDefinition(bool includeAlias = true, bool useHistory = false, bool useTranslation = false)
        {
            return GetPostgresDefinition(_definition, includeAlias, useHistory, useTranslation);
        }
        private string GetPostgresDefinition(DbDefinition dbDefinition, bool includeAlias = true, bool useHistory = false, bool useTranslation = false)
        {
            // If Definition.Plantform == "Mssql" => Qualify names [..]
            string res = "";
            if (useHistory)
            {
                if (useTranslation)
                {
                    res = $"{dbDefinition.TranslationHistorySchema}.{dbDefinition.BaseType.Name}";
                }
                else
                {
                    res = $"{dbDefinition.BaseHistorySchema}.{dbDefinition.BaseType.Name}";
                }
            }
            else
            {
                if (useTranslation)
                {
                    res = $"{dbDefinition.TranslationSchema}.{dbDefinition.BaseType.Name}";
                }
                else
                {
                    res = $"{dbDefinition.BaseSchema}.{dbDefinition.BaseType.Name}";
                }
            }

            if (includeAlias)
            {
                res += $" AS {dbDefinition.BaseType.Name}";
            }

            return res;
        }

        /*Basic*/
        private string GenerateColumns(RequestOptions options)
        {
            bool useTranslation = !string.IsNullOrEmpty(options.Language) && _definition.HasTranslation;
            var columns = new List<string>();

            foreach (var p in _definition.Columns.Select(t => t.Property))
            {
                if (useTranslation && p.PropertyType == typeof(string))
                {
                    columns.Add($"coalesce(T_{_definition.BaseType.Name}.{p.Name}, {_definition.BaseType.Name}.{p.Name}) AS {p.Name}");
                }
                else
                {
                    columns.Add($"{_definition.BaseType.Name}.{p.Name} AS {p.Name}");
                }
            }

            // Add paging row number if needed
            if (options.UsePaging)
            {
                string orderBy = !string.IsNullOrEmpty(options.OrderBy) && _definition.Columns.Exists(t => t.Name.Equals(options.OrderBy, StringComparison.CurrentCultureIgnoreCase)) ? options.OrderBy : "Id";
                columns.Add($"ROW_NUMBER() OVER (ORDER BY {_definition.BaseType.Name}.{orderBy}) AS _rownum");
            }

            return string.Join(',', columns);
        }
        private string GenerateSource(RequestOptions options)
        {
            bool useTranslation = !string.IsNullOrEmpty(options.Language) && _definition.HasTranslation;
            bool useHistory = options.AsOf.HasValue;

            if (useTranslation)
            {
                // Example for translation JOIN
                return $@"""
                {GetPostgresDefinition(includeAlias: true, useHistory: useHistory)}
                LEFT JOIN LATERAL (SELECT * FROM {GetPostgresDefinition(includeAlias: false, useTranslation: true, useHistory: useHistory)} AS T 
                WHERE T.Id = {_definition.BaseType.Name}.Id AND T.Language = @Language ) AS T_{_definition.BaseType.Name} ON 1=1
                """;
            }
            else
            {
                return GetPostgresDefinition(useHistory: useHistory);
            }
        }
        private string GenerateFilterStatement(string tableAlias, IEnumerable<GenericFilter>? filters)
        {
            if (filters == null || !filters.Any())
            {
                return string.Empty;
            }

            var conditions = new List<string>();

            foreach (var filter in filters)
            {
                string condition = filter.Comparer switch
                {
                    FilterComparer.Equals => $"{tableAlias}.{filter.PropertyName} = @{filter.PropertyName}",
                    FilterComparer.NotEqual => $"{tableAlias}.{filter.PropertyName} <> @{filter.PropertyName}",
                    FilterComparer.GreaterThan => $"{tableAlias}.{filter.PropertyName} > @{filter.PropertyName}",
                    FilterComparer.GreaterThanOrEqual => $"{tableAlias}.{filter.PropertyName} >= @{filter.PropertyName}",
                    FilterComparer.LessThan => $"{tableAlias}.{filter.PropertyName} < @{filter.PropertyName}",
                    FilterComparer.LessThanOrEqual => $"{tableAlias}.{filter.PropertyName} <= @{filter.PropertyName}",
                    FilterComparer.StartsWith => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                    FilterComparer.EndsWith => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                    FilterComparer.Contains => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                    _ => throw new NotSupportedException($"Comparer '{filter.Comparer}' is not supported.")
                };

                conditions.Add(condition);
            }

            return conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;
        }
        private string AddPagingToQuery(string query, RequestOptions options)
        {
            if (!options.UsePaging)
            {
                return query;
            }

            var sb = new StringBuilder();
            sb.AppendLine("WITH pagedresult AS (");
            sb.AppendLine(query);
            sb.AppendLine(")");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM pagedresult, (SELECT MAX(pagedresult._rownum) AS totalitems FROM pagedresult) AS pageinfo");
            sb.AppendLine($"ORDER BY _rownum OFFSET {options.PageSize * (options.PageNumber - 1)} ROWS FETCH NEXT {options.PageSize} ROWS ONLY");

            return sb.ToString();
        }

        /*Extended*/
        private string GetJoinPostgresStatement(ForeignKeyDefinition join, RequestOptions options)
        {
            var joinDef = DefinitionStore.TryGetDefinition(join.Ref) ?? throw new InvalidOperationException();
            bool useHistory = options.AsOf.HasValue;

            var sb = new StringBuilder();

            sb.Append($"{(join.IsOptional ? "LEFT OUTER" : "INNER")} JOIN {GetPostgresDefinition(joinDef, includeAlias: false, useHistory: useHistory)} AS _{join.ExtendedProperty} ON {join.Base.Name}.{join.BaseProperty} = _{join.ExtendedProperty}.{join.RefProperty} {GetJoinPostgresFilterString(join)}");

            bool useTranslation = !string.IsNullOrEmpty(options.Language);
            if (useTranslation && joinDef.HasTranslation)
            {
                sb.AppendLine();
                sb.Append($"LEFT JOIN LATERAL (SELECT * FROM {GetPostgresDefinition(joinDef, useTranslation: true, includeAlias: false, useHistory: useHistory)} AS T ");
                sb.Append($"WHERE T.Id = _{join.ExtendedProperty}.Id AND t.Language = @Language) AS T_{join.ExtendedProperty} ON 1=1");
            }

            return sb.ToString();
        }
        private string GenerateJoinPostgresColumns(ForeignKeyDefinition join, RequestOptions options)
        {
            var joinDef = DefinitionStore.TryGetDefinition(join.Ref);
            if (joinDef == null)
            {
                return string.Empty;
            }

            if (!join.IsList)
            {
                bool useTranslation = !string.IsNullOrEmpty(options.Language);
                var columns = new List<string>();
                foreach (var p in joinDef.Columns.Select(t => t.Property))
                {
                    if (joinDef.HasTranslation && useTranslation && p.PropertyType == typeof(string))
                    {
                        columns.Add($"coalesce(T_{join.ExtendedProperty}.{p.Name}, _{join.ExtendedProperty}.{p.Name}) AS {join.ExtendedProperty}_{p.Name}");
                        //columns.Add($"coalesce({join.Alias}{join.JoinObj.TranslationDbObject.Alias}.{p.Name},_{join.Alias}.{p.Name}) AS {join.Alias}_{p.Name}");
                    }
                    else
                    {
                        columns.Add($"_{join.ExtendedProperty}.{p.Name} AS {join.ExtendedProperty}_{p.Name}");
                    }
                }

                return string.Join(',', columns);
            }
            else
            {
                return $"COALESCE((SELECT JSON_AGG(ROW_TO_JSON({join.ExtendedProperty})) FROM {GetPostgresDefinition(joinDef, includeAlias: false)} AS {join.ExtendedProperty} WHERE {join.ExtendedProperty}.{join.RefProperty} = {join.Base.Name}.{join.BaseProperty}), '[]') AS {join.ExtendedProperty}";
            }
        }
        private string GetJoinPostgresFilterString(ForeignKeyDefinition join)
        {
            if (join.Filters == null || join.Filters.Count == 0)
            {
                return "";
            }

            string result = string.Empty;

            foreach (var filter in join.Filters)
            {
                result += $" AND {join.Base.Name}.{filter.PropertyName} = _{join.ExtendedProperty}.{filter.Value}";
            }

            return result;
        }

        /*Write*/
        private string UpdateSetStatement(IEnumerable<NpgsqlParameter> parameters)
        {
            return UpdateSetStatement(parameters.Select(t => t.ParameterName).ToList());
        }
        private string UpdateSetStatement(List<string> values)
        {
            return string.Join(',', values.OrderBy(t => t).Select(t => $"{t} = @{t}").ToList());
        }
        private string InsertColumns(List<NpgsqlParameter> values)
        {
            return InsertColumns(values.Select(t => t.ParameterName));
        }
        private string InsertColumns(IEnumerable<string> values)
        {
            return string.Join(',', values.OrderBy(t => t).Select(t => $"{t}").ToList());
        }
        private string InsertValues(List<NpgsqlParameter> values)
        {
            return InsertValues(values.Select(t => t.ParameterName));
        }
        private string InsertValues(IEnumerable<string> values)
        {
            return string.Join(',', values.OrderBy(t => t).Select(t => $"@{t}").ToList());
        }
    }
}

