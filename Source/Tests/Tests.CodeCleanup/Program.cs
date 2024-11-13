namespace Tests.CodeCleanup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            ReturnOne();

            TestIf(true);
        }

        static int ReturnOne()
        {
            return 1;
        }

        static void TestIf(bool? a)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a == true)
            {
                Console.WriteLine("True");
            }
            else
            {
                Console.WriteLine("False");
            }
        }
    }
}

