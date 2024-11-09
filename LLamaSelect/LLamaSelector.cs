using System.Runtime.InteropServices;

namespace LLamaSelect
{
    public static class LLamaSelector
    {
        [DllImport("CUDA/cudart64_12.dll")]
        private static extern int cudaDriverGetVersion(out int version);

        public static string GetLLamaPath()
        {
            cudaDriverGetVersion(out int driverVersion);

            if (driverVersion >= 12200)
                return "CUDA/llama.dll";
            else
                return "Vulkan/llama.dll";
        }
    }
}
