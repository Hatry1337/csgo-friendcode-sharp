using System;

namespace CSGOFriendCodeConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Should be SUCVS-FADA
            Console.WriteLine(FriendCode.encode("76561197960287930"));

            // should be 76561197960287930
            Console.WriteLine(FriendCode.decode("SUCVS-FADA"));
        }
    }
}