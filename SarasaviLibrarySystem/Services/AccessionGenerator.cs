using System;
using Microsoft.Data.Sqlite;
using SarasaviLibrarySystem.Data;

namespace SarasaviLibrarySystem.Services
{
    public static class AccessionGenerator
    {
        public static string GenerateNextAccessionCode(char classificationCode)
        {
            char codeChar = char.ToUpper(classificationCode);
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT AccessionCode FROM BookTitles WHERE ClassificationCode = @cls ORDER BY TitleId DESC;";
            cmd.Parameters.AddWithValue("@cls", codeChar.ToString());

            int maxIndex = 0;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string code = reader.GetString(0);
                if (code.Length >= 5 && code[0] == codeChar && int.TryParse(code.Substring(1), out int index))
                {
                    if (index > maxIndex) maxIndex = index;
                }
            }

            int nextIndex = maxIndex + 1;
            return $"{codeChar}{nextIndex:D4}";
        }

        public static string GenerateCopyAccessionNumber(string baseAccessionCode, int copyIndex)
        {
            if (copyIndex < 1 || copyIndex > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(copyIndex), "Copy index must be between 1 and 10.");
            }
            return $"{baseAccessionCode}-{copyIndex:D2}";
        }
    }
}
