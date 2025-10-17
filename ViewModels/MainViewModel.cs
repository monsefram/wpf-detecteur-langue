using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using tpfred2.Models;
using tpfred2.ViewModels.Commands;
using tpfred2.Views;

namespace tpfred2.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ====== Champs ======
        private readonly SettingsStore _settings = new();
        private ApiClient? _api;                              
        private Dictionary<string, string>? _langMap;         // code -> nom long

        private string _inputText = string.Empty;
        private DetectionItem? _selected;

        // ====== Propriétés UI ======
        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested(); 
                }
            }
        }

        public ObservableCollection<DetectionItem> Detections { get; } = new();

        public DetectionItem? SelectedDetection
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedLanguage));
                    OnPropertyChanged(nameof(SelectedConfidence));
                    OnPropertyChanged(nameof(SelectedReliable));
                }
            }
        }

        public string SelectedLanguage => SelectedDetection?.LongName ?? "";
        public string SelectedConfidence => SelectedDetection?.Confidence.ToString("0.###") ?? "";
        public string SelectedReliable => (SelectedDetection?.IsReliable ?? false) ? "Oui" : "Non";

        public RelayCommand ShowConfigCommand { get; }
        public RelayCommand ShowStatusCommand { get; }
        public AsyncCommand DetectCommand { get; }

        public MainViewModel()
        {
            ShowConfigCommand = new RelayCommand(_ => OpenConfig(), _ => true);
            ShowStatusCommand = new RelayCommand(_ => ShowStatus(), _ => true);
            DetectCommand = new AsyncCommand(_ => DetectAsync(),
                                                 _ => !string.IsNullOrWhiteSpace(InputText)
                                                   && !string.IsNullOrWhiteSpace(_settings.Current.ApiToken));

        }

        private ApiClient GetApi()
        {
            if (_api != null) return _api;
            _api = new ApiClient("https://ws.detectlanguage.com/v3/");   
            _api.SetHttpRequestHeader("Authorization", "Bearer " + _settings.Current.ApiToken);
            return _api;
        }


        private void OpenConfig()
        {
            var w = new ConfigWindow
            {
                DataContext = new ConfigViewModel(_settings),
                Owner = Application.Current.MainWindow
            };
            w.ShowDialog();
            // rafraîchir CanExecute (bouton détecter dépend aussi du token)
            CommandManager.InvalidateRequerySuggested();
            // si l'API existait, remettre le header (au cas où le token a changé)
            if (_api != null)
                _api.SetHttpRequestHeader("Authorization", "Bearer " + _settings.Current.ApiToken);
        }

        // ----------- Détection (POST) -----------
        private async Task DetectAsync()
        {
            try
            {
                var api = GetApi();

                if (_langMap == null)
                {
                    var bodyLang = await api.RequeteGetAsync("languages"); // <-- v3
                    using var docLang = JsonDocument.Parse(bodyLang);
                    _langMap = ParseLanguagesFlexible(docLang.RootElement);
                }

                var body = await api.RequetePostJsonAsync("detect", new { q = InputText });
                using var doc = JsonDocument.Parse(body);
                var detArray = FindDetectionsArray(doc.RootElement);

                if (detArray.ValueKind != JsonValueKind.Array)
                    throw new FormatException("Réponse inattendue de l'API (aucun tableau de détections).");


                Detections.Clear();
                foreach (var d in detArray.EnumerateArray())
                {
                    string code = d.TryGetProperty("language", out var pLang) ? (pLang.GetString() ?? "")
                                : d.TryGetProperty("code", out var pCode) ? (pCode.GetString() ?? "")
                                : "";

                    double score = d.TryGetProperty("score", out var pScore) && pScore.ValueKind == JsonValueKind.Number
                                     ? pScore.GetDouble()
                                     : (d.TryGetProperty("confidence", out var pConf) && pConf.ValueKind == JsonValueKind.Number
                                         ? d.GetProperty("confidence").GetDouble() / 100.0
                                         : 0.0);

                    // règle "Est fiable"score > 0.5
                    bool rel = score >= 0.5;

                    Detections.Add(new DetectionItem
                    {
                        LongName = (_langMap != null && _langMap.TryGetValue(code, out var longName))
                                        ? longName : code.ToUpperInvariant(),
                        Confidence = score,
                        IsReliable = rel
                    });
                }
                SelectedDetection = Detections.FirstOrDefault();

            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message, "HTTP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur inattendue : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ----------- Statut (GET) -----------
        private static int GetInt(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
                if (obj.TryGetProperty(n, out var v) && v.TryGetInt32(out var i)) return i;
            return 0;
        }
        private static long GetLong(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
                if (obj.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.Number) return v.GetInt64();
            return 0;
        }
        private static string GetStr(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
                if (obj.TryGetProperty(n, out var v) && v.ValueKind != JsonValueKind.Null) return v.GetString() ?? "";
            return "";
        }

        private async void ShowStatus()
        {
            if (string.IsNullOrWhiteSpace(_settings.Current.ApiToken))
            {
                MessageBox.Show("Veuillez configurer le jeton d’abord.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var api = GetApi();
                var body = await api.RequeteGetAsync("account/status"); // <-- v3

                using var doc = JsonDocument.Parse(body);
                var d = doc.RootElement.TryGetProperty("data", out var dd) ? dd : doc.RootElement;

                var data = new StatusData
                {
                    date = GetStr(d, "date"),
                    // certains JSON renvoient "requests" au lieu de "requests_today"
                    requests_today = GetInt(d, "requests_today", "requests"),
                    // idem "bytes" vs "bytes_today"
                    bytes_today = GetLong(d, "bytes_today", "bytes"),
                    plan = GetStr(d, "plan"),
                    plan_expires = GetStr(d, "plan_expires"),
                    daily_requests_limit = GetInt(d, "daily_requests_limit", "requests_limit", "daily_requests"),
                    daily_bytes_limit = GetLong(d, "daily_bytes_limit", "bytes_limit", "daily_bytes"),
                    status = GetStr(d, "status")
                };

                var vm = new TokenStatusViewModel(data);
                var w = new TokenStatusWindow { DataContext = vm, Owner = Application.Current.MainWindow };
                w.ShowDialog();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Jeton non valide ou erreur HTTP.\n" + ex.Message,
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur inattendue : " + ex.Message,
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static Dictionary<string, string> ParseLanguagesFlexible(JsonElement root)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in root.EnumerateArray())
                {
                    var code = el.TryGetProperty("code", out var c) ? (c.GetString() ?? "") : "";
                    var name = el.TryGetProperty("name", out var n) ? (n.GetString() ?? code) : code;
                    if (!string.IsNullOrWhiteSpace(code))
                        map[code] = name.ToUpperInvariant();
                }
                return map;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                    return ParseLanguagesFlexible(data);

                if (root.TryGetProperty("languages", out var langs) && langs.ValueKind == JsonValueKind.Array)
                    return ParseLanguagesFlexible(langs);

                bool looksLikeMap = true;
                foreach (var p in root.EnumerateObject())
                {
                    if (p.Value.ValueKind != JsonValueKind.String) { looksLikeMap = false; break; }
                    map[p.Name] = p.Value.GetString()!.ToUpperInvariant();
                }
                if (looksLikeMap) return map;
            }

            throw new FormatException("Réponse inattendue de /languages.");
        }

        private static JsonElement FindDetectionsArray(JsonElement e)
        {
            static bool LooksLikeDetection(JsonElement x)
                => x.ValueKind == JsonValueKind.Object &&
                   (x.TryGetProperty("language", out _) || x.TryGetProperty("code", out _));

            switch (e.ValueKind)
            {
                case JsonValueKind.Array:
                    if (e.EnumerateArray().Any(LooksLikeDetection)) return e;
                    foreach (var it in e.EnumerateArray())
                    {
                        var sub = FindDetectionsArray(it);
                        if (sub.ValueKind == JsonValueKind.Array) return sub;
                    }
                    break;

                case JsonValueKind.Object:
                    if (e.TryGetProperty("detections", out var dets))
                    {
                        var sub = FindDetectionsArray(dets);
                        if (sub.ValueKind == JsonValueKind.Array) return sub;
                    }
                    if (e.TryGetProperty("data", out var data))
                    {
                        var sub = FindDetectionsArray(data);
                        if (sub.ValueKind == JsonValueKind.Array) return sub;
                    }
                    foreach (var p in e.EnumerateObject())
                    {
                        var sub = FindDetectionsArray(p.Value);
                        if (sub.ValueKind == JsonValueKind.Array) return sub;
                    }
                    break;
            }
            return default;
        }
    }
}
