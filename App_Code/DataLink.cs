﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

public static class DataLink
{
    public struct OperationResault
    {
        public bool succeded;
        public string message;

        public OperationResault(bool succeded, string message)
        {
            this.succeded = succeded;
            this.message = message;
        }
    }

    public static bool LogIn(string email, string password)
    {
        string sql = string.Format("SELECT * FROM Users WHERE Email='{0}' AND PassHash='{1}'", email, GetHashedPassword(password));
        return MyAdoHelper.IsExist(database, sql);
    }

    public static OperationResault AddUser(string email, string fname, string lname, string password, string id)
    {
        if (IsEmailRegistered(email))
            return new OperationResault(false, "The email you are using has already registered");
        if (IsIDRegistered(id))
            return new OperationResault(false, "The id you are using has already registered");


        string sql = string.Format("INSERT INTO Users (Email, ID, FirstName, LastName, PassHash) VALUES ('{0}','{1}','{2}','{3}', '{4}')",
            email, id, fname, lname, GetHashedPassword(password));
        MyAdoHelper.DoQuery(database, sql);

        if (ConfirmRegistration(email, fname, lname, password, id))
            return new OperationResault(true, "Success");
        else
            return new OperationResault(false, "Database Error");
    }

    public static OperationResault UpdateEmail(string oldEmail ,string newEmail)
    {
        if (IsEmailRegistered(newEmail))
            return new OperationResault(false, "The email you are using has already registered");

        string sql = string.Format("UPDATE Users SET Email='{0}' WHERE Email='{1}'", newEmail, oldEmail);
        MyAdoHelper.DoQuery(database, sql);

        if (IsEmailRegistered(newEmail))
            return new OperationResault(true, "The email has been changed");
        else
            return new OperationResault(false, "Database Error");
    }

    public static OperationResault UpdatePassword(string email, string newPassword)
    {
        string sql = string.Format("UPDATE Users SET PassHash='{0}' WHERE email='{1}'", GetHashedPassword(newPassword), email);
        MyAdoHelper.DoQuery(database, sql);
        string select = string.Format("SELECT * FROM USERS WHERE Email='{0}' AND PassHash='{1}'", email, GetHashedPassword(newPassword));
        if (MyAdoHelper.IsExist(database, select))
            return new OperationResault(true, "The password has been changed");
        else
            return new OperationResault(false, "Database Error");
    }

    public static DataTable GetAllUsers()
    {
        return MyAdoHelper.ExecuteDataTable("Database.mdf", "SELECT * FROM Users");
    }

    public static void Delete(string email)
    {
        string sql = string.Format("DELETE FROM Users WHERE Email='{0}'", email);
        MyAdoHelper.DoQuery(database, sql);
    }

    private static string database = "Database.mdf";

    private static bool IsEmailRegistered(string email)
    {
        string sql = string.Format("SELECT * FROM Users WHERE Email='{0}'", email);
        return MyAdoHelper.IsExist(database, sql);
    }

    private static bool IsIDRegistered(string id)
    {
        string sql = string.Format("SELECT * FROM Users WHERE ID='{0}'", id);
        return MyAdoHelper.IsExist(database, sql);
    }

    private static bool ConfirmRegistration(string email, string fname, string lname, string password, string id)
    {
        string sql = string.Format("SELECT * FROM Users WHERE Email='{0}' AND ID='{1}' AND FirstName='{2}' AND LastName='{3}' AND PassHash='{4}'",
            email, id, fname, lname, GetHashedPassword(password));
        return MyAdoHelper.IsExist(database, sql);
    }

    private static string GetHashedPassword(string unhashed)
    {
        string salt = "c5g3uatKK6HECzAqK9uuUr5Xs6hF9n3X";
        return GetHashString(unhashed + salt);
    }

    private static string GetHashString(string unhashed)
    {
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(unhashed);

        System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();

        byte[] hash = sha.ComputeHash(inputBytes);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < hash.Length; i++)
        {

            sb.Append(hash[i].ToString("x2"));

        }

        return sb.ToString();

    }
}