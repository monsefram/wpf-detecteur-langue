# wpf-detecteur-langue

**FR** — Application de bureau **WPF (C#)** qui **détecte la langue** d'un texte saisi en interrogeant une API REST de détection linguistique, puis affiche les langues candidates et leur probabilité.

**EN** — A **WPF (C#)** desktop app that **detects the language** of an input text by calling a REST language-detection API, then displays the candidate languages and their probabilities.

---

## Fonctionnalités / Features

- **FR**
  - Détection de langue en temps réel via un client HTTP (`ApiClient`).
  - Architecture **MVVM** (ViewModels, commandes, vues).
  - Correspondance code → nom de langue, résultats triés par probabilité.
  - URL de l'API et réglages conservés localement (`SettingsStore`).
- **EN**
  - Real-time language detection via an HTTP client (`ApiClient`).
  - **MVVM** architecture (ViewModels, commands, views).
  - Language code → full name mapping, results ranked by probability.
  - API URL and settings stored locally (`SettingsStore`).

## Stack

C# · .NET · WPF · MVVM · `HttpClient` · `System.Text.Json`.

## Configuration & lancement / Setup & run

**FR** : renseigner l'URL de base de l'API de détection dans les réglages de l'application (aucune clé n'est stockée dans le dépôt).
**EN**: set the detection API base URL in the app settings (no key is committed).

```bash
# Ouvrir tpfred2.sln dans Visual Studio 2022, puis F5
dotnet run
```
