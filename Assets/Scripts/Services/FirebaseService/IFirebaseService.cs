using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFirebaseService
{
    FirebaseAuth Auth { get; }
    FirebaseFirestore DB { get; }
    FirebaseUser CurrentUser { get; }
    UserAccountModel CurrentUserData { get; }
    
    Task<bool> InitializeAsync();
}