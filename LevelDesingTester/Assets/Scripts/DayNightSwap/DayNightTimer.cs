using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightTimer : MonoBehaviour
{
  private static DayNightTimer _instance;

    // Propiedad para acceder a la instancia del Singleton
    public static DayNightTimer Instance
    {
        get
        {
            // Si no hay una instancia válida, intenta encontrarla en la escena
            if (_instance == null)
            {
                _instance = FindObjectOfType<DayNightTimer>();

                // Si no se encuentra, crea una nueva instancia en la escena
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(DayNightTimer).Name);
                    _instance = singletonObject.AddComponent<DayNightTimer>();
                }
            }

            return _instance;
        }
    }


    public Volume volume; // Asigna el volumen que contiene el color grading
    public Color startColor;
    public Color endColor;
    public float duration = 120f; // Duración de la transición de color
    public float delayTime = 540f; // Tiempo de retraso antes de comenzar el cambio de color (9 minutos en segundos)

    private ColorAdjustments colorAdjustments;
    public float elapsedTime = 0f;
    private bool delayCompleted = false;
    
    void Start()
    {
        if (volume == null)
        {
            Debug.LogError("Volume not assigned!");
            return;
        }

        // Obtén el componente de ajuste de color
        volume.profile.TryGet(out colorAdjustments);

        // Establece el color inicial
        colorAdjustments.colorFilter.value = startColor;
    }

    void Update()
    {
        if (colorAdjustments == null)
            return;

        // Si el tiempo de retraso aún no se ha completado, aumenta el tiempo transcurrido
        if (!delayCompleted)
        {
            elapsedTime += Time.deltaTime;

            // Comprueba si se ha completado el tiempo de retraso
            if (elapsedTime >= delayTime)
            {
                delayCompleted = true;
                elapsedTime = 0f; // Restablece el tiempo transcurrido para la transición de color
            }
            return; // Sale del Update para evitar que el cambio de color se active antes de tiempo
        }

        // Una vez que se haya completado el tiempo de retraso, comienza la transición de color
        // Actualiza el tiempo transcurrido
        elapsedTime += Time.deltaTime;

        // Interpola entre los colores inicial y final
        float t = Mathf.Clamp01(elapsedTime / duration);
        Color newColor = Color.Lerp(startColor, endColor, t);

        // Aplica el nuevo color al filtro de color
        colorAdjustments.colorFilter.value = newColor;

        // Reinicia el temporizador si ha pasado la duración
        //if (elapsedTime >= duration)
            //elapsedTime = 0f;
    }

     private void Awake()
    {
        // Si hay otra instancia existente, destruye esta para mantener solo una
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Marca esta instancia como la única instancia válida
        _instance = this;

        // Asegúrate de que este objeto no se destruya cuando se cargue una nueva escena
        DontDestroyOnLoad(gameObject);
    }
}