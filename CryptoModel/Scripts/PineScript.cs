namespace CryptoModel.Scripts
{
    internal class PineScript
    {
        /// <summary>
        /// arr[index] is valid, return that value
        /// arr[index] is invalid, return 0
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static float Nz(float[] arr, int index)
        {
            return index >= 0 && index < arr.Length ? arr[index] : 0;
        }
    }
}
