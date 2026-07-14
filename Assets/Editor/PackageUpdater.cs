using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class PackageUpdater
{
    static ListRequest _listRequest;
    static Queue<string> _updateQueue;
    static AddRequest _addRequest;
    static int _total;
    static int _done;

    [MenuItem("Tools/Update All Packages")]
    static void Run()
    {
        // offlineMode: false → 레지스트리에서 최신 버전 정보 조회
        _listRequest = Client.List(offlineMode: false, includeIndirectDependencies: false);
        EditorApplication.update += WaitForList;
        EditorUtility.DisplayProgressBar("Package Updater", "패키지 목록 조회 중...", 0f);
    }

    static void WaitForList()
    {
        if (!_listRequest.IsCompleted) return;
        EditorApplication.update -= WaitForList;

        if (_listRequest.Status != StatusCode.Success)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"[PackageUpdater] 목록 조회 실패: {_listRequest.Error.message}");
            return;
        }

        _updateQueue = new Queue<string>();

        foreach (var pkg in _listRequest.Result)
        {
            if (pkg.source == PackageSource.BuiltIn || pkg.source == PackageSource.Embedded)
                continue;

            if (pkg.source == PackageSource.Git)
            {
                // packageId = "name@url" 형식 — 동일 URL로 재추가하면 최신 커밋으로 갱신됨
                _updateQueue.Enqueue(pkg.packageId);
                Debug.Log($"[PackageUpdater] Git 패키지 갱신 예정: {pkg.name}");
            }
            else if (!string.IsNullOrEmpty(pkg.versions.latest) && pkg.versions.latest != pkg.version)
            {
                _updateQueue.Enqueue($"{pkg.name}@{pkg.versions.latest}");
                Debug.Log($"[PackageUpdater] 업데이트 예정: {pkg.name}  {pkg.version} → {pkg.versions.latest}");
            }
        }

        if (_updateQueue.Count == 0)
        {
            EditorUtility.ClearProgressBar();
            Debug.Log("[PackageUpdater] 모든 패키지가 최신 상태입니다.");
            return;
        }

        _total = _updateQueue.Count;
        _done = 0;
        Debug.Log($"[PackageUpdater] 총 {_total}개 패키지 업데이트 시작");
        ProcessNext();
    }

    static void ProcessNext()
    {
        if (_updateQueue.Count == 0)
        {
            EditorUtility.ClearProgressBar();
            Debug.Log($"[PackageUpdater] 완료 ({_done}/{_total}개)");
            return;
        }

        var id = _updateQueue.Dequeue();
        EditorUtility.DisplayProgressBar("Package Updater", $"업데이트 중: {id}", (float)_done / _total);
        _addRequest = Client.Add(id);
        EditorApplication.update += WaitForAdd;
    }

    static void WaitForAdd()
    {
        if (!_addRequest.IsCompleted) return;
        EditorApplication.update -= WaitForAdd;

        if (_addRequest.Status == StatusCode.Success)
        {
            _done++;
            Debug.Log($"[PackageUpdater] ✓ {_addRequest.Result.name}@{_addRequest.Result.version}");
        }
        else
        {
            Debug.LogWarning($"[PackageUpdater] 실패: {_addRequest.Error.message}");
        }

        ProcessNext();
    }
}
