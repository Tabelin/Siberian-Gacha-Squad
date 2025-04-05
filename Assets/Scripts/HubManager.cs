// HubManager.cs
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // Префаб для персонажа
    public GameObject characterPrefab;

    // Список точек спавна
    public Transform[] spawnPoints;

    // Атласы аниматоров для фонов
    public RuntimeAnimatorController commonBackgroundController;
    public RuntimeAnimatorController rareBackgroundController;
    public RuntimeAnimatorController epicBackgroundController;
    public RuntimeAnimatorController legendaryBackgroundController;

    void Start()
    {
        if (characterPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab и SpawnPoints!");
            return;
        }

        // Загружаем сохранённых персонажей из PlayerPrefs
        List<Character> savedCharacters = LoadCharactersFromPlayerPrefs();

        // Спавним каждого персонажа в точке хаба
        for (int i = 0; i < savedCharacters.Count && i < spawnPoints.Length; i++)
        {
            SpawnCharacter(savedCharacters[i], spawnPoints[i]);
        }
    }

    // Метод для загрузки персонажей из PlayerPrefs
    private List<Character> LoadCharactersFromPlayerPrefs()
    {
        List<Character> characters = new List<Character>();

        string json = PlayerPrefs.GetString("SaveData", "");
        if (!string.IsNullOrEmpty(json))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            foreach (CharacterData data in saveData.characters)
            {
                Character loadedCharacter = new Character
                {
                    name = data.name,
                    rarity = data.rarity,
                    health = data.health,
                    attack = data.attack,
                    defense = data.defense,
                    carryWeight = data.carryWeight,
                    level = data.level,
                    maxLevel = data.maxLevel
                };

                characters.Add(loadedCharacter); // Добавляем персонажа в список
                Debug.Log($"Персонаж загружен: {loadedCharacter.name} ({loadedCharacter.rarity})");
            }
        }
        else
        {
            Debug.Log("Нет сохранённых персонажей.");
        }

        return characters;
    }

    // Метод для спавна персонажа
    private void SpawnCharacter(Character character, Transform spawnPoint)
    {
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Необходимо назначить CharacterPrefab или SpawnPoint!");
            return;
        }

        // Создаём объект персонажа
        GameObject characterObject = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        if (characterObject != null)
        {
            // Настройка основного спрайта персонажа
            SpriteRenderer mainSpriteRenderer = characterObject.GetComponent<SpriteRenderer>();
            mainSpriteRenderer.sprite = character.sprite;

            // Добавляем анимированный фон через Animator
            AddAnimatedBackground(characterObject, character.rarity);

            Debug.Log($"Персонаж {character.name} ({character.rarity}) успешно спавнится в хабе!");
        }
        else
        {
            Debug.LogError("Не удалось создать CharacterPrefab!");
        }
    }

    // Метод для добавления анимированного фона через Animator
    private void AddAnimatedBackground(GameObject parentObject, Rarity rarity)
    {
        if (parentObject == null)
        {
            Debug.LogError("Переданный parentObject является null!");
            return;
        }

        // Создаём объект для фона
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.parent = parentObject.transform;
        backgroundObject.transform.localPosition = Vector3.zero;

        // Добавляем Sprite Renderer для фона
        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = GetDefaultBackgroundSprite(rarity);
        backgroundRenderer.sortingOrder = -1; // Фон находится позади основного спрайта

        // Добавляем Animator компонент
        Animator animator = backgroundObject.AddComponent<Animator>();
        SetAnimatorController(animator, rarity);

        // Настройка размера фона
        backgroundRenderer.transform.localScale = new Vector3(1.2f, 1.2f, 1f); // Фон чуть больше основного спрайта
    }

    // Метод для получения заглушечного спрайта фона
    private Sprite GetDefaultBackgroundSprite(Rarity rarity)
    {
        // Получаем спрайт фона через GachaSystem
        if (GachaSystem.Instance != null)
        {
            return GachaSystem.Instance.GetBackgroundSprite(rarity);
        }
        else
        {
            Debug.LogError("GachaSystem не найден!");
            return null;
        }
    }

    // Метод для установки Animator Controller
    private void SetAnimatorController(Animator animator, Rarity rarity)
    {
        if (animator == null)
        {
            Debug.LogError("Переданный animator является null!");
            return;
        }

        RuntimeAnimatorController controller = rarity switch
        {
            Rarity.Common => commonBackgroundController,
            Rarity.Rare => rareBackgroundController,
            Rarity.Epic => epicBackgroundController,
            Rarity.Legendary => legendaryBackgroundController,
            _ => null
        };

        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning($"Анимационный контроллер для редкости {rarity} не найден!");
        }
    }

    
}