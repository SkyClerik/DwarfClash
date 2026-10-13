using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Game3DValidator : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_EDITOR
        // Запускаем все наши проверки только в редакторе
        if (!ValidateClickSystem())
        {
            // Если проверка системы кликов провалилась, останавливаем игру
            Debug.LogError("Game3DValidator: Критическая ошибка в настройке сцены! Запуск остановлен.");
            EditorApplication.isPlaying = false;
            return;
        }

        //
        // Сюда можно будет добавлять другие методы валидации
        // if (!ValidateSomethingElse()) { ... }
        //
#endif
    }

    /// <summary>
    /// Проверяет, что система кликов по 3D-объектам настроена корректно.
    /// </summary>
    /// <returns>True, если всё в порядке.</returns>
    private bool ValidateClickSystem()
    {
        // 1. Проверяем наличие EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("Game3DValidator: В сцене отсутствует EventSystem! Клики по 3D объектам не будут работать. Добавьте его через UI -> Event System.");
            return false;
        }

        // 2. Проверяем наличие PhysicsRaycaster на основной камере
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Game3DValidator: Не найдена камера с тегом 'MainCamera'! Невозможно проверить наличие PhysicsRaycaster.");
            return false;
        }

        if (mainCamera.GetComponent<PhysicsRaycaster>() == null)
        {
            Debug.LogError($"Game3DValidator: На основной камере '{mainCamera.name}' отсутствует PhysicsRaycaster! Клики по 3D объектам не будут работать. Добавьте компонент PhysicsRaycaster на камеру.");
            return false;
        }

        Debug.Log("Game3DValidator: Проверка системы кликов прошла успешно!");
        return true;
    }

    //
    // Здесь можно будет добавлять другие методы проверок...
    //
}
