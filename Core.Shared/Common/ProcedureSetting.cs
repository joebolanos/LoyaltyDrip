using Dapper;

namespace Core.Shared.Common
{
    public class ProcedureSetting
    {
        public string Name { get; set; }
        public DynamicParameters Params { get; set; }
    }
}
