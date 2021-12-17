# csgo-friendcode-sharp

Based on the [js version](https://github.com/emily33901/js-csfriendcode) that [Emily](https://github.com/emily33901/) wrote. This takes a steamid64 and turns it into a CSGO friend code or takes a friend code and turns it into a steamid64.

```cs
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
            
            //Also implemented other methods from original module, check out https://github.com/emily33901/js-csfriendcode
        }
    }
}
```
