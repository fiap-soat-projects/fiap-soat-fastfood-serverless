namespace fiap.soat.fastfood.serverless.function
{
    public static class CpfUtils
    {
        public static bool IsValidCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf)) return false;
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) return false;
            if (new string(cpf[0], 11) == cpf) return false;
            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf = cpf.Substring(0, 9);
            int sum = 0;
            for (int i = 0; i < 9; i++) sum += int.Parse(tempCpf[i].ToString()) *
            mult1[i];
            int remainder = sum % 11;
            int dig1 = remainder < 2 ? 0 : 11 - remainder;
            tempCpf = tempCpf + dig1.ToString();
            sum = 0;
            for (int i = 0; i < 10; i++) sum += int.Parse(tempCpf[i].ToString()) *
            mult2[i];
            remainder = sum % 11;
            int dig2 = remainder < 2 ? 0 : 11 - remainder;
            return cpf.EndsWith(dig1.ToString() + dig2.ToString());
        }
    }
}
