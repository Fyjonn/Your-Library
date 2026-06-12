# Your-Library

## How to run the project locally

1. Clone the repository or fork it.
2. In the project's root directory, you'll find the `appsettings.Example.json` file.
3. Rename this file to `appsettings.json`.
4. Create your own API key in the [Google Cloud Console](https://console.cloud.google.com/) for the **Google Books API** service:
   * Log in to your Google account
   * Create a project (upper left corner)
   * Make sure you are working on the right project
   * Go to `APIs and Services` -> `Enabled APIs and Services`
   * Click `Enable APIs and Services`
   * Search `Books API`
   * Click on the result
   * Enable it
   * Go to `APIs and Services` -> `Login details`
   * Click `Create login details` -> `API key`
   * Choose a name for your API key
   * In the `Select API constraints` select `Books API`
   * Click `Create`
   * Your API key sould be displayed
5. Paste your key in the `appsettings.json` file in place of `"YOUR_GOOGLE_BOOKS_API_KEY_HERE"`.
6. Configure email service
   (If you eventually want to omit this step and don't use this option go step 7.)

   This functionality covers resetting your password through an email.
   Configure it followingly:

   * Log into your Google account
   * Enable 2-Step Verification
   * Go to Google Account -> Security -> App Passwords
   * Create a new App Password
   * Copy the generated password
   * Fill the `EmailSettings` section in `appsettings.json`, by writing down  your email and pasting password

7. Omitting configuring email service (Only if you skipped step 6.)
   Warning - without configuring this functionality, button "Forgot Password" won't work properly and you won't be able to use retrievieng password optiton.
   We still recommend to go back and go through step 6. If you decided to skip it though follow this step:

   * In `appsettings.json` file delete this part:

       "EmailSettings": {
     
      "FromEmail": "",
   
      "SmtpServer": "smtp.gmail.com",
     
      "Port": 587,
   
      "Username": "",
   
      "Password": ""
   
      }

    Remember from now on button "Forgot Passwordd" will not work and clicking it will result in application error. But you can still do step 6. later.
     
8. Migrate the database in the Package Manager Console:

```text
Update-Database
