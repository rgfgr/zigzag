using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using Firebase.Database;
using System.Linq;

public class FirebaseManager : MonoBehaviour
{

    #region Variables

    #region Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference dBReference;
    private static FirebaseManager instance;
    public static FirebaseManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
            {
                instance = value;
            }
            else
            {
                Destroy(value);
            }
        }
    }
    #endregion

    #region Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    #endregion

    #region Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    #endregion

    public GameObject scoreElement;
    public Transform scoreboardContent;

    #endregion


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch(scene.buildIndex)
        {
            case 1:
                LoadMainMenu();
                break;
            case 2:
                LoadGameScene();
                break;
            default:
                break;
        }
    }

    private void LoadGameScene()
    {
        GameObject.Find("AddScore").GetComponent<Button>().onClick.AddListener(SaveDataButton);
        scoreboardContent = GameObject.Find("ScoreboardContent").GetComponent<Transform>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        //Check that all of the necessary dependecies for Firebase are present on the system
        StartCoroutine(CheckAndFixDependenciesAsync());
    }
    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            //If they are avalible Initialize Firebase
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoLogin());
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependecies: " + dependencyStatus);
        }
    }

    private IEnumerator CheckForAutoLogin()
    {
        if (User != null)
        {
            var reloadUserTask = User.ReloadAsync();
            yield return new WaitUntil(() => reloadUserTask.IsCompleted);
        }
        SceneManager.LoadScene(1);
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        dBReference = FirebaseDatabase.DefaultInstance.RootReference;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != User)
        {
            bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && User != null)
            {
                Debug.Log("Signed out " + User.UserId);
            }

            User = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + User.UserId);
            }
        }
    }

    #region Main menu

    private void LoadMainMenu()
    {
        scoreboardContent = GameObject.Find("ScoreboardContent").GetComponent<Transform>();
        emailLoginField = GameObject.Find("Email_Input").GetComponent<TMP_InputField>();
        passwordLoginField = GameObject.Find("Password_Input").GetComponent<TMP_InputField>();
        warningLoginText = GameObject.Find("Warning_Text").GetComponent<TextMeshProUGUI>();
        usernameRegisterField = GameObject.Find("Username_Input").GetComponent<TMP_InputField>();
        emailRegisterField = GameObject.Find("Email_Reg_Input").GetComponent<TMP_InputField>();
        passwordRegisterField = GameObject.Find("Password_Reg_Input").GetComponent<TMP_InputField>();
        passwordRegisterVerifyField = GameObject.Find("Confirm_Input").GetComponent<TMP_InputField>();
        warningRegisterText = GameObject.Find("Warning_Reg_Text").GetComponent<TextMeshProUGUI>();
        GameObject.Find("Register_Reg_Btn").GetComponent<Button>().onClick.AddListener(RegisterButton);
        GameObject.Find("Sign Out").GetComponent<Button>().onClick.AddListener(SignOutButton);
        GameObject.Find("Login_Btn").GetComponent<Button>().onClick.AddListener(LoginButton);
        if (User != null)
        {
            MainMenuController.instance.MainMenu();
        }
        else
        {
            MainMenuController.instance.LoginScreen();
        }
    }

    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    //Function for the sign out button
    public void SignOutButton()
    {
        auth.SignOut();
        MainMenuController.instance.LoginScreen();
        ClearRegisterFields();
        ClearLoginFeilds();
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var loginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the tash completes
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning($"Failed to register task with {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserMismatch:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = loginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            MainMenuController.instance.MainMenu();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (string.IsNullOrWhiteSpace(_username))
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
            yield break;
        }

        if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
            yield break;
        }

        //Call the Firebase auth signin function passing the email and password
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning($"Failed to register task with {registerTask.Exception}");
            FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Register Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WeakPassword:
                    message = "Weak Password";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "Email Already In Use";
                    break;
            }
            warningRegisterText.text = message;
            yield break;
        }

        //User has now been created
        //Now get the result
        User = registerTask.Result;

        if (User != null)
        {
            //Create a user profile and set the username
            UserProfile profile = new() { DisplayName = _username };

            //Call the Firebase auth update user profile function passing the profile with the username
            var profileTask = User.UpdateUserProfileAsync(profile);
            //Wait until the tash completes
            yield return new WaitUntil(() => profileTask.IsCompleted);

            if (profileTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning($"Faild to register task with {profileTask.Exception}");
                FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                warningRegisterText.text = "Username Set Failed!";
            }
            else
            {
                //Username is now set
                //Now return to login screen
                MainMenuController.instance.MainMenu();
            }
        }
    }

    #endregion

    #region Save data

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new() { DisplayName = _username };

        //Call the Firebase auth update user profile function passing the profile with the username
        var profileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(() => profileTask.IsCompleted);

        if (profileTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {profileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        var dBTask = dBReference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(() => dBTask.IsCompleted);

        if(dBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {dBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(User.DisplayName));
        StartCoroutine(UpdateUsernameDatabase(User.DisplayName));

        StartCoroutine(UpdateScore(GameMaster.instance.Points));
    }

    private IEnumerator UpdateScore(int _points)
    {
        var dBTask = dBReference.Child("users").Child(User.UserId).Child("score").SetValueAsync(_points);

        yield return new WaitUntil(() => dBTask.IsCompleted);

        if(dBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {dBTask.Exception}");
        }
        else
        {
            LoadBoard();
        }
    }
    #endregion

    #region Load data

    public void LoadBoard()
    {
        StartCoroutine(LoadScoreboardData());
    }

    private IEnumerator LoadScoreboardData()
    {
        var dBTask = dBReference.Child("users").OrderByChild("score").GetValueAsync();

        yield return new WaitUntil(() => dBTask.IsCompleted);

        if(dBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {dBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = dBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            int element = 1;
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse())
            {
                string username = childSnapshot.Child("username").Value.ToString();
                int score = int.Parse(childSnapshot.Child("score").Value.ToString());

                //Instantiate new scoreboard elements
                GameObject scoreboardElement = Instantiate(scoreElement,
                    scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, score);
                element++;
            }
        }
    }
    #endregion
}
