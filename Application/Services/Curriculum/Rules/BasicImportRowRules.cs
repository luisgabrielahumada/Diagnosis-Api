using Application.Services.Curriculum.Models;

namespace Application.Services.Curriculum.Validate
{
    public static class BasicImportRowRules
    {
        /// <summary>
        /// 1) Excluye filas con Descripcion vacía/nula.
        /// 2) Si PeriodId no tiene valor (null o Guid.Empty), asigna periodIdToAssign.
        /// Devuelve NUEVA lista (no toca el origen).
        /// </summary>
        public static List<BasicImportRow> Normalize(this List<BasicImportRow>? source, Guid? periodIdToAssign)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source
                .Where(r => !string.IsNullOrWhiteSpace(r?.Descripcion)) 
                .Select(r =>
                {
                    r.Descripcion = r.Descripcion.Trim();

                    if (!r.PeriodId.HasValue || r.PeriodId == Guid.Empty)
                        r.PeriodId = periodIdToAssign;

                    return r;
                })
                .ToList();
        }

        /// <summary>
        /// Misma lógica que arriba, pero modifica la LISTA original en sitio.
        /// </summary>
        public static void NormalizeInPlace(this List<BasicImportRow>? rows,Guid? periodIdToAssign)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var r = rows[i];
                var desc = r?.Descripcion?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(desc))
                {
                    rows.RemoveAt(i);
                    continue;
                }
                r.Descripcion = desc;
                if (!r.PeriodId.HasValue || r.PeriodId == Guid.Empty)
                    r.PeriodId = periodIdToAssign;
            }
        }
    }
}
