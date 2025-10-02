# .NET Aspire 앱을 Azure Container Apps에 배포하기

.NET Aspire는 컨테이너화된 환경에서 실행되도록 설계된 애플리케이션에 최적화되어 있습니다. [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview)는 서버리스 플랫폼에서 마이크로서비스와 컨테이너화된 애플리케이션을 실행할 수 있는 완전 관리형 환경입니다. 이 문서에서는 Visual Studio와 Azure Developer CLI(`azd`)를 사용하여 새로운 .NET Aspire 솔루션을 생성하고 Microsoft Azure Container Apps에 배포하는 과정을 안내합니다.

이 예제에서는 이전 섹션의 MyWeatherHub 앱을 배포한다고 가정합니다. 구축한 코드를 사용하거나 **complete** 디렉토리의 코드를 사용할 수 있습니다. 하지만 일반적인 단계는 모든 .NET Aspire 앱에 대해 동일합니다.

## Visual Studio로 앱 배포

1. 솔루션 탐색기에서 **AppHost** 프로젝트를 마우스 오른쪽 버튼으로 클릭하고 **게시**를 선택하여 **게시** 대화 상자를 엽니다.

    > .NET Aspire 게시에는 현재 버전의 `azd` CLI가 필요합니다. 이는 .NET Aspire 워크로드와 함께 설치되어야 하지만, CLI가 설치되지 않았거나 최신 상태가 아니라는 알림을 받으면 이 튜토리얼의 다음 부분에 있는 지침을 따라 설치할 수 있습니다.

1. 게시 대상으로 **Azure Container Apps for .NET Aspire**를 선택합니다.

    ![게시 대화 상자 워크플로의 스크린샷.](../media/vs-deploy.png)

1. **AzDev Environment** 단계에서 원하는 **구독** 및 **위치** 값을 선택한 다음 _aspire-weather_와 같은 **환경 이름**을 입력합니다. 환경 이름은 Azure Container Apps 환경 리소스의 명명을 결정합니다.
1. **마침**을 선택하여 환경을 생성한 다음 **닫기**를 선택하여 대화 상자 워크플로를 종료하고 배포 환경 요약을 확인합니다.
1. **게시**를 선택하여 Azure에서 리소스를 프로비저닝하고 배포합니다.

    > 이 프로세스는 완료하는 데 몇 분이 걸릴 수 있습니다. Visual Studio는 출력 로그에서 배포 진행률에 대한 상태 업데이트를 제공하며, 이러한 업데이트를 관찰하여 게시 작동 방식에 대해 많은 것을 배울 수 있습니다! 프로세스에는 리소스 그룹, Azure Container Registry, Log Analytics 작업 영역 및 Azure Container Apps 환경 생성이 포함됨을 알 수 있습니다. 그런 다음 앱이 Azure Container Apps 환경에 배포됩니다.

1. 게시가 완료되면 Visual Studio는 환경 화면 하단에 리소스 URL을 표시합니다. 이러한 링크를 사용하여 배포된 다양한 리소스를 확인합니다. **webfrontend** URL을 선택하여 배포된 앱의 브라우저를 엽니다.

    ![완료된 게시 프로세스 및 배포된 리소스의 스크린샷.](../media/vs-publish-complete.png)

## Azure Developer CLI(azd)로 앱 배포

### Azure Developer CLI 설치

`azd` 설치 프로세스는 운영 체제에 따라 다르지만 `winget`, `brew`, `apt` 또는 `curl`을 통해 직접 널리 사용할 수 있습니다. `azd`를 설치하려면 [Azure Developer CLI 설치](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)를 참조하세요.

### .NET Aspire 9.4의 새로운 기능: aspire deploy 명령

.NET Aspire 9.4는 게시 기능을 확장하여 대상 환경에 적극적으로 배포하는 `aspire deploy` 명령(미리 보기/기능 플래그)을 도입합니다. 이 명령은 사용자 지정 배포 전/후 로직으로 향상된 배포 워크플로를 제공합니다.

이 기능을 활성화하려면:

```bash
aspire config set features.deployCommandEnabled true
```

그런 다음 다음을 사용할 수 있습니다:

```bash
aspire deploy
```

이 명령은 향상된 진행률 보고, 더 나은 오류 메시지 및 복잡한 배포 시나리오를 위한 사용자 지정 배포 후크 지원을 제공합니다.

### 템플릿 초기화

> 전제 조건:
>
> - 로그인되어 있는지 확인하세요: `azd login`을 실행하고 올바른 Azure 구독을 선택합니다.
> - AppHost가 포함된 폴더에서 다음 명령을 실행합니다(이 리포지토리의 경우 완성된 샘플을 배포하는 경우 일반적으로 `complete` 폴더).

1. 새 터미널 창을 열고 .NET Aspire 프로젝트의 루트로 `cd`합니다.
1. `azd init` 명령을 실행하여 `azd`로 프로젝트를 초기화합니다. 이는 로컬 디렉터리 구조를 검사하고 앱 유형을 결정합니다.

    ```console
    azd init
    ```

    `azd init` 명령에 대한 자세한 내용은 [azd init](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-init)을 참조하세요.
1. 처음으로 앱을 초기화하는 경우 `azd`는 환경 이름을 묻습니다:

    ```console
    Azure에서 실행할 앱 초기화 (azd init)
    
    ? 새 환경 이름을 입력하세요: [? for help]
    ```

    계속하려면 원하는 환경 이름을 입력합니다. `azd`로 환경 관리에 대한 자세한 내용은 [azd env](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-env)를 참조하세요.
1. `azd`가 두 가지 앱 초기화 옵션을 제시하면 **현재 디렉터리의 코드 사용**을 선택합니다.

    ```console
    ? 앱을 어떻게 초기화하시겠습니까?  [화살표로 이동, 입력으로 필터링]
    > 현재 디렉터리의 코드 사용
      템플릿 선택
    ```

1. 디렉터리를 스캔한 후 `azd`는 올바른 .NET Aspire _AppHost_ 프로젝트를 찾았는지 확인하도록 요청합니다. **확인하고 내 앱 초기화 계속** 옵션을 선택합니다.

    ```console
    감지된 서비스:
    
      .NET (Aspire)
      감지 위치: D:\source\repos\letslearn-dotnet-aspire\complete\AppHost\AppHost.csproj
    
    azd는 Azure Container Apps를 사용하여 Azure에서 앱을 호스팅하는 데 필요한 파일을 생성합니다.
    
    ? 옵션을 선택하세요  [화살표로 이동, 입력으로 필터링]
    > 확인하고 내 앱 초기화 계속
      취소하고 종료
    ```

1. `azd`는 .NET Aspire 솔루션의 각 프로젝트를 표시하고 모든 인터넷 트래픽에 공개적으로 열린 HTTP 수신으로 배포할 프로젝트를 식별하도록 요청합니다. API(`api`)를 Azure Container Apps 환경에 비공개로 두고 공개적으로 사용할 수 없도록 하려면 `myweatherhub`만 선택합니다(↓ 및 스페이스 키 사용).

    ```console
    ? 옵션을 선택하세요 확인하고 내 앱 초기화 계속
    기본적으로 서비스는 실행 중인 Azure Container Apps 환경 내부에서만 도달할 수 있습니다. 여기서 서비스를 선택하면 인터넷에서도 도달할 수 있습니다.
    ? 인터넷에 노출할 서비스를 선택하세요  [화살표로 이동, 스페이스로 선택, <오른쪽>으로 모두, <왼쪽>으로 없음, 입력으로 필터링]
          [ ]  api
        > [x]  myweatherhub
    ```

1. 마지막으로 Azure에서 프로비저닝된 리소스의 이름을 지정하고 `dev` 및 `prod`와 같은 다양한 환경을 관리하는 데 사용되는 환경 이름을 지정합니다.

    ```console
    Azure에서 앱을 실행하기 위한 파일 생성:
    
      (✓) 완료: ./azure.yaml 생성
      (✓) 완료: ./next-steps.md 생성
    
    성공: 앱이 클라우드를 위해 준비되었습니다!
    이 디렉터리에서 azd up 명령을 실행하여 Azure에서 앱을 프로비저닝하고 배포할 수 있습니다. 앱 구성에 대한 자세한 내용은 ./next-steps.md를 참조하세요
    ```

`azd`는 여러 파일을 생성하여 작업 디렉터리에 배치합니다. 이 파일들은:

- _azure.yaml_: .NET Aspire AppHost 프로젝트와 같은 앱의 서비스를 설명하고 Azure 리소스에 매핑합니다.
- _.azure/config.json_: 현재 활성 환경이 무엇인지 `azd`에 알려주는 구성 파일입니다.
- _.azure/aspireazddev/.env_: 환경별 재정의를 포함합니다.
- _.azure/aspireazddev/config.json_: 이 환경에서 공용 끝점을 가져야 하는 서비스를 `azd`에 알려주는 구성 파일입니다.

### 앱 배포

`azd`가 초기화되면 프로비저닝 및 배포 프로세스를 단일 명령인 [azd up](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-up)으로 실행할 수 있습니다.

```console
기본적으로 서비스는 실행 중인 Azure Container Apps 환경 내부에서만 도달할 수 있습니다. 여기서 서비스를 선택하면 인터넷에서도 도달할 수 있습니다.
? 인터넷에 노출할 서비스를 선택하세요 webfrontend
? 사용할 Azure 구독을 선택하세요:  1. <당신의 구독>
? 사용할 Azure 위치를 선택하세요: 1. <당신의 위치>

서비스 패키징 (azd package)


성공: 애플리케이션이 1초 이내에 Azure용으로 패키지되었습니다.

Azure 리소스 프로비저닝 (azd provision)
Azure 리소스 프로비저닝에는 시간이 걸릴 수 있습니다.

구독: <당신의 구독>
위치: <당신의 위치>

  Azure Portal에서 자세한 진행률을 볼 수 있습니다:
<배포 링크>

  (✓) 완료: 리소스 그룹: <당신의 리소스 그룹>
  (✓) 완료: Container Registry: <ID>
  (✓) 완료: Log Analytics 작업 영역: <ID>
  (✓) 완료: Container Apps 환경: <ID>
  (✓) 완료: Container App: <ID>

성공: 애플리케이션이 1분 13초 만에 Azure에 프로비저닝되었습니다.
Azure Portal에서 리소스 그룹 <당신의 리소스 그룹> 아래에 생성된 리소스를 볼 수 있습니다:
<리소스 그룹 개요 링크>

서비스 배포 (azd deploy)

  (✓) 완료: 서비스 api 배포
  - 끝점: <내부 전용>

  (✓) 완료: 서비스 myweatherhub 배포
  - 끝점: <당신의 고유한 myweatherhub 앱>.azurecontainerapps.io/


성공: 애플리케이션이 1분 39초 만에 Azure에 배포되었습니다.
Azure Portal에서 리소스 그룹 <당신의 리소스 그룹> 아래에 생성된 리소스를 볼 수 있습니다:
<리소스 그룹 개요 링크>

성공: Azure에서 프로비저닝하고 배포하는 up 워크플로가 3분 50초 만에 완료되었습니다.
```

먼저 프로젝트는 `azd package` 단계에서 컨테이너로 패키지되고, 이어서 앱이 필요로 하는 모든 Azure 리소스가 프로비저닝되는 `azd provision` 단계가 진행됩니다.

`provision`이 완료되면 `azd deploy`가 실행됩니다. 이 단계에서 프로젝트는 컨테이너로 Azure Container Registry 인스턴스에 푸시되고, 코드가 호스팅될 Azure Container Apps의 새 수정 버전을 만드는 데 사용됩니다.

이 시점에서 앱이 배포되고 구성되었으며, Azure Portal을 열고 리소스를 탐색할 수 있습니다.

## 리소스 정리

생성한 Azure 리소스가 더 이상 필요하지 않을 때 다음 Azure CLI 명령을 실행하여 리소스 그룹을 삭제합니다. 리소스 그룹을 삭제하면 그 안에 포함된 리소스도 삭제됩니다.

```console
az group delete --name <당신의-리소스-그룹-이름>
```

**다음**: [모듈 #10: 고급 컨테이너 관리](../Lesson-10-Container-Management/README.md)
