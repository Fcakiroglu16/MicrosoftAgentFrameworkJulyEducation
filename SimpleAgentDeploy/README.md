# Foundry Agent Yayınlama Rehberi

Bu doküman, `SimpleAgentDeploy` uygulamasının Docker ile paketlenip Azure Container Registry (ACR) üzerinden Azure AI Foundry'ye bir agent olarak yayınlanması için gereken adımları anlatır.

## Ön Koşullar

- Azure hesabı ve aktif bir abonelik
- Azure AI Foundry'de oluşturulmuş bir **Foundry** kaynağı ve bu kaynak altında bir **proje**
- Bir **Azure Container Registry** (örnek: `julyeducation`)
- Docker Desktop (yerelde kurulu ve çalışır durumda)

## 1. Azure Foundry ve Proje Oluşturma

1. [Azure AI Foundry](https://ai.azure.com) portalına gidin.
2. Yeni bir **Foundry** kaynağı oluşturun.
3. Foundry kaynağı içinde yeni bir **proje** oluşturun.

## 2. Azure Container Registry Oluşturma

Azure Portal veya CLI üzerinden bir Container Registry oluşturun (örnek isim: `julyeducation`).

```bash
az acr create --resource-group <resource-group-adi> --name julyeducation --sku Basic
```

## 3. Azure CLI Kurulumu (macOS)

```bash
brew update && brew install azure-cli
```

## 4. Azure'a Giriş Yapma

```bash
az login
```

## 5. Foundry için Access Token Alma

Foundry servislerine erişim için gerekli erişim token'ını alın:

```bash
az account get-access-token --resource https://ai.azure.com --query accessToken -o tsv
```

## 6. Docker İmajını Build Etme

Proje kök dizininde (`SimpleAgentDeploy` klasörü, `Dockerfile`'ın bulunduğu yer) aşağıdaki komutu çalıştırın:

```bash
docker build -t simple-agent:v1 .
```

> **Not (Apple Silicon / macOS):** Azure Container Instance/App Service gibi ortamlar genellikle `linux/amd64` mimarisini bekler. M1/M2/M3 Mac kullanıyorsanız, imajı doğrudan hedef platform ve tag ile build edin:
>
> ```bash
> docker build --platform linux/amd64 -t julyeducation.azurecr.io/simple-agent:v2 .
> ```

## 7. Azure Container Registry'e Giriş Yapma

```bash
az acr login --name julyeducation
```

## 8. İmajı Etiketleme (Tag)

```bash
docker tag simple-agent:v1 julyeducation.azurecr.io/simple-agent:v1
```

## 9. İmajı ACR'a Push Etme

```bash
docker push julyeducation.azurecr.io/simple-agent:v1
```

### macOS (Apple Silicon) için Alternatif Akış

Build sırasında hedef platform belirtildiği için ayrı bir `tag` adımına gerek kalmaz:

```bash
docker build --platform linux/amd64 -t julyeducation.azurecr.io/simple-agent:v2 .
docker push julyeducation.azurecr.io/simple-agent:v2
```

## 10. Sonraki Adım: Foundry Agent Deployment

İmaj ACR'a push edildikten sonra, Azure AI Foundry projenizde bu container imajını referans alarak agent deployment'ını oluşturabilir/güncelleyebilirsiniz (Foundry portalı üzerinden veya `azd`/Azure CLI ile).

## Özet Komut Akışı

```bash
# 1. Azure CLI kurulumu
brew update && brew install azure-cli

# 2. Giriş yap
az login

# 3. Token al
az account get-access-token --resource https://ai.azure.com --query accessToken -o tsv

# 4. Build
docker build -t simple-agent:v1 .

# 5. ACR login
az acr login --name julyeducation

# 6. Tag
docker tag simple-agent:v1 julyeducation.azurecr.io/simple-agent:v1

# 7. Push
docker push julyeducation.azurecr.io/simple-agent:v1

# macOS (Apple Silicon) için:
docker build --platform linux/amd64 -t julyeducation.azurecr.io/simple-agent:v2 .
docker push julyeducation.azurecr.io/simple-agent:v2
```
