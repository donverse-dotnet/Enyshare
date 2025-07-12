using System;

namespace MyGrpcApp.Service
{
    public class AuthorizationService
    {
        public bool IsActionAllowed(string role, string resouce, string action)
        {
            //仮メール：adminはすべて許可、userはreadのみ許可
            if (role == "admin") return true;
            if (role == "user" && action == "read") return true;

            return false;
        }
    }
}