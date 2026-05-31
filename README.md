# Your-Library

## How to run the project locally

1. Clone the repository or fork it.
2. In the project's root directory, you'll find the `appsettings.Example.json` file.
3. Rename this file to `appsettings.json`.
4. Create your own API key in the [Google Cloud Console](https://console.cloud.google.com/) for the **Google Books API** service.
5. Paste your key in the `appsettings.json` file in place of `"YOUR_GOOGLE_BOOKS_API_KEY_HERE"`.
6. Migrate the database in the Package Manager Console:

```text
Update-Database
