namespace Application.Strategies.Helper
{
    public static class ZombieDetector
    {
        public static bool IsZombie(this string[] geneticCode)
        {
            int n = geneticCode.Length;
            if (n == 0) return false;

            char[][] matrix = geneticCode.Select(r => r.ToCharArray()).ToArray();

            int found = 0;

            // Direcciones: → abajo, → derecha, ↘ diagonal, ↙ diagonal inversa
            int[][] directions = new int[][]
            {
                new[] { 0, 1 },  // horizontal
                new[] { 1, 0 },  // vertical
                new[] { 1, 1 },  // diagonal principal
                new[] { 1, -1 }  // diagonal inversa
            };

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    char current = matrix[r][c];

                    foreach (var d in directions)
                    {
                        int dr = d[0];
                        int dc = d[1];

                        // posición final del grupo de 4 letras
                        int endR = r + 3 * dr;
                        int endC = c + 3 * dc;

                        if (endR < 0 || endR >= n || endC < 0 || endC >= n)
                            continue;

                        bool match = true;

                        for (int k = 1; k < 4; k++)
                        {
                            if (matrix[r + k * dr][c + k * dc] != current)
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            found++;
                            if (found > 1) return true;
                        }
                    }
                }
            }

            return false;
        }

    }

}
