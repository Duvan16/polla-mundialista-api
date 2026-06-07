using BCrypt.Net;
class HashGen {
    static void Main(string[] args) {
        Console.WriteLine(BCrypt.HashPassword(args[0]));
    }
}
