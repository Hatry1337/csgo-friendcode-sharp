using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CSGOFriendCodeConverter
{
    public class FriendCode
    {
        private static BigInteger default_steam_id = 0x110000100000000;
        private static BigInteger default_group_id = 0x170000000000000;
        private static string alnum = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private static Dictionary<char, BigInteger> ralnum = new Dictionary<char, BigInteger>
        {
            ['A'] = 0,
            ['B'] = 1,
            ['C'] = 2,
            ['D'] = 3,
            ['E'] = 4,
            ['F'] = 5,
            ['G'] = 6,
            ['H'] = 7,
            ['J'] = 8,
            ['K'] = 9,
            ['L'] = 10,
            ['M'] = 11,
            ['N'] = 12,
            ['P'] = 13,
            ['Q'] = 14,
            ['R'] = 15,
            ['S'] = 16,
            ['T'] = 17,
            ['U'] = 18,
            ['V'] = 19,
            ['W'] = 20,
            ['X'] = 21,
            ['Y'] = 22,
            ['Z'] = 23,
            ['2'] = 24,
            ['3'] = 25,
            ['4'] = 26,
            ['5'] = 27,
            ['6'] = 28,
            ['7'] = 29,
            ['8'] = 30,
            ['9'] = 31
        };
        
        private static string b32(string input)
        {
            string res = "";
            // Make input into a big endian
            BigInteger bigEndian = ByteSwap.from_big_endian(ByteSwap.to_little_endian(BigInteger.Parse(input)));

            for (int i = 0; i < 13; i++) {
                if (i == 4 || i == 9)
                {
                    res += "-";
                }

                res += alnum[(char) (bigEndian & 0x1F)];
                bigEndian >>= 5;
            }
            return res;
        }
        
        private static  BigInteger rb32(string input)
        {
            BigInteger res = 0;

            for (int i = 0; i < 13; i++) {
                if (i == 4 || i == 9)
                {
                    input = input.Substring(1);
                }
                res |= ralnum[input[0]] << (5 * i);
                input = input.Substring(1);
            }

            return ByteSwap.from_big_endian(ByteSwap.to_little_endian(res));
        }
        
        private static BigInteger hash_steam_id(BigInteger id)
        {
            BigInteger account_id = id & 0xFFFFFFFF;
            BigInteger strange_steam_id = account_id | 0x4353474F00000000;

            byte[] bytes = ByteSwap.to_little_endian(strange_steam_id);
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes).Take(4).ToArray();
                return ByteSwap.from_little_endian(hash);
            }
        }
        
        private static BigInteger make_u64(BigInteger hi,BigInteger lo) {
            return hi << 32 | lo;
        }
        
        public static string encode(string steamid) {
            BigInteger id = BigInteger.Parse(steamid);

            BigInteger h = hash_steam_id(id);

            BigInteger r = 0;
            for (int i = 0; i < 8; i++)
            {
                BigInteger id_nibble = id & 0xF;
                id >>= 4;
                
                BigInteger hash_nibble = (h >> i) & 1;
                BigInteger a = r << 4 | id_nibble;

                r = make_u64(r >> 28, a);
                r = make_u64(r >> 31, a << 1 | hash_nibble);
            }

            string res = b32(r.ToString());

            // Check if it begins with AAAA- and remove if it does

            if (res.Substring(0, 4) == "AAAA")
            {
                res = res.Substring(5);
            }

            return res;
        }
        
        public static string encode(BigInteger id) {
            BigInteger h = hash_steam_id(id);

            BigInteger r = 0;
            for (int i = 0; i < 8; i++)
            {
                BigInteger id_nibble = id & 0xF;
                id >>= 4;
                
                BigInteger hash_nibble = (h >> i) & 1;
                BigInteger a = r << 4 | id_nibble;

                r = make_u64(r >> 28, a);
                r = make_u64(r >> 31, a << 1 | hash_nibble);
            }

            string res = b32(r.ToString());

            // Check if it begins with AAAA- and remove if it does

            if (res.Substring(0, 4) == "AAAA")
            {
                res = res.Substring(5);
            }

            return res;
        }
        
        private static BigInteger? __decode(string friend_code) {
            if (friend_code.Length != 10) return null;

            if (friend_code.Substring(0, 5) != "AAAA-")
            {
                friend_code = "AAAA-" + friend_code;
            }

            BigInteger val = rb32(friend_code);
            BigInteger id = 0;

            for (int i = 0; i < 8; i++)
            {
                val >>= 1;
                BigInteger id_nibble = val & 0xF;
                val >>= 4;

                id <<= 4;
                id |= id_nibble;
            }

            return id;
        }
        
        public static string decode(string friend_code)
        {
            BigInteger? id = FriendCode.__decode(friend_code);

            if (id != null)
            {
                return (id | default_steam_id).ToString();
            }

            return "";
        }
        
        public static string encode_direct_challenge(string account_id) {
            BigInteger id = BigInteger.Parse(account_id);
            Random random = new Random();
            BigInteger r()
            {
                return (BigInteger) Math.Floor(random.NextDouble() * 0x7fff) << 16;
            }

            string part1 = FriendCode.encode(r() | (id & 0x0000FFFF));
            string part2 = FriendCode.encode(r() | ((id & 0xFFFF0000) >> 16));

            return $"{part1}-{part2}";
        }
        
        public static string encode_direct_group_challenge(string group_id)
        {
            BigInteger id = BigInteger.Parse(group_id);
            string part1 = FriendCode.encode(0x10000 | (id & 0x0000FFFF));
            string part2 = FriendCode.encode(0x10000 | ((id & 0xFFFF0000) >> 16));

            return $"{part1}-{part2}";
        }
        
        public static string decode_direct_challenge(string challenge_code)
        {
            if (challenge_code.Length != 21) return "";

            BigInteger part1 = (BigInteger) FriendCode.__decode(challenge_code.Substring(0, 10));
            BigInteger part2 = (BigInteger) FriendCode.__decode(challenge_code.Substring(11));

            string type = "u";
            BigInteger id = (part1 & 0x0000FFFF) | ((part2 & 0x0000FFFF) << 16);

            if ((part1 & 0xFFFF0000) == 0x10000 && (part2 & 0xFFFF0000) == 0x10000)
            {
                type = "g";
                id = id | default_group_id;
            } else
            {
                id = id | default_steam_id;
            }

            return $"{part1},{part2},{type},{id}";
        }
    }
}