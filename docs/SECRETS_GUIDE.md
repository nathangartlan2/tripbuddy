# TripBuddy API - Secrets Configuration Guide

## 🔐 Secure API Key Management

This project uses multiple layers of configuration to keep your API keys secure and out of version control.

## 🛠️ Setup Methods (Choose One)

### Method 1: .NET User Secrets (Recommended for Development)

1. **Initialize User Secrets**

   ```bash
   cd tripbuddy
   dotnet user-secrets init
   ```

2. **Set Your API Keys**

   ```bash
   # Set OpenAI API Key
   dotnet user-secrets set "OpenAI:ApiKey" "sk-your-actual-openai-key-here"

   # Optional: Set custom LLAMA endpoint
   dotnet user-secrets set "Llama:ApiUrl" "http://localhost:11434/api/generate"
   dotnet user-secrets set "Llama:Model" "llama2"
   ```

3. **Verify Secrets**
   ```bash
   dotnet user-secrets list
   ```

### Method 2: Environment Variables

Set environment variables in your shell:

```bash
# Bash/Zsh
export OpenAI__ApiKey="sk-your-actual-openai-key-here"
export Llama__ApiUrl="http://localhost:11434/api/generate"
export Llama__Model="llama2"

# PowerShell
$env:OpenAI__ApiKey="sk-your-actual-openai-key-here"
$env:Llama__ApiUrl="http://localhost:11434/api/generate"
$env:Llama__Model="llama2"
```

### Method 3: Local appsettings.Development.json (Not Committed)

Create `appsettings.Development.json` (already in .gitignore):

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-openai-key-here"
  },
  "Llama": {
    "ApiUrl": "http://localhost:11434/api/generate",
    "Model": "llama2"
  }
}
```

### Method 4: .env File (Not Committed)

Create a `.env` file (already in .gitignore):

```bash
OPENAI__APIKEY=sk-your-actual-openai-key-here
LLAMA__APIURL=http://localhost:11434/api/generate
LLAMA__MODEL=llama2
```

## 🔧 Configuration Priority

.NET loads configuration in this order (later sources override earlier ones):

1. `appsettings.json` (base settings)
2. `appsettings.{Environment}.json`
3. User Secrets (development only)
4. Environment Variables
5. Command-line arguments

## 🚀 Production Deployment

### Azure App Service

```bash
az webapp config appsettings set --resource-group myResourceGroup --name myapp --settings OpenAI__ApiKey="your-key"
```

### Docker

```yaml
version: "3.8"
services:
  tripbuddy:
    image: tripbuddy:latest
    environment:
      - OpenAI__ApiKey=${OPENAI_API_KEY}
      - Llama__ApiUrl=${LLAMA_API_URL}
    env_file:
      - .env.production
```

### GitHub Actions Secrets

1. Go to your repository → Settings → Secrets and variables → Actions
2. Add repository secrets:
   - `OPENAI_API_KEY`
   - `LLAMA_API_URL`

```yaml
# .github/workflows/deploy.yml
env:
  OpenAI__ApiKey: ${{ secrets.OPENAI_API_KEY }}
  Llama__ApiUrl: ${{ secrets.LLAMA_API_URL }}
```

## 🔍 Verifying Configuration

The API will log configuration status on startup. Check for:

```
info: TripBuddy.Program[0]
      OpenAI API Key: Configured ✓
      LLAMA API URL: Configured ✓
```

If keys are missing, you'll see:

```
warn: TripBuddy.Program[0]
      OpenAI API Key: NOT CONFIGURED - API will fail
```

## 🚨 Security Best Practices

### ✅ DO:

- Use User Secrets for local development
- Use environment variables in production
- Keep `.env` files in `.gitignore`
- Rotate API keys regularly
- Use different keys for dev/staging/prod

### ❌ DON'T:

- Commit API keys to version control
- Share secrets in chat/email
- Use production keys in development
- Log API keys in application output

## 🧪 Testing Configuration

```bash
# Test with User Secrets
dotnet user-secrets set "OpenAI:ApiKey" "test-key"
dotnet run

# Test with Environment Variables
OpenAI__ApiKey="test-key" dotnet run

# Verify API responds
curl -k https://localhost:5001/api/search/health
```

## 📁 File Structure

```
tripbuddy/
├── appsettings.json                    # ✅ Safe to commit (no secrets)
├── appsettings.Development.json        # ❌ In .gitignore
├── .env                               # ❌ In .gitignore
├── .gitignore                         # ✅ Protects secrets
└── SECRETS_GUIDE.md                   # ✅ This guide
```

## 🔄 Migration from Current Setup

If you already have keys in `appsettings.json`:

1. **Copy your keys** before continuing
2. **Remove them** from `appsettings.json`
3. **Set them using Method 1** (User Secrets)
4. **Verify** the app still works
5. **Commit** the cleaned `appsettings.json`

---

**Your API keys are now secure and won't be committed to Git!** 🔐
