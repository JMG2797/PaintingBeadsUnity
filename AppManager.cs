using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; //CON ESTO PODEMOS CREAR VARIABLES DE UI
using UnityEngine.EventSystems;  //CON ESTO PODEMOS CREAR VARIABLES DE 
using UnityEngine;

public class AppManager : MonoBehaviour
{
    //un menú es una máquina de estados. Hay varios estados posibles, pero solo puede haber una activo, nunca más de uno a la vez
    public enum InteractionMode
    {
        Paint, Erase, Pick, Fill, Pattern
    }

    public InteractionMode currentMode;

    public enum PatternEnum
    {
        Horizontal, Vertical, Square, Cross
    }

    public PatternEnum currentPattern;

    [Header("App Elements")]
    public RectTransform mainCanvas;
    public RectTransform mainPanel;
    public GameObject bead;
    public Transform boardParent;
    public Transform ToolParent;

    public List<Image> beadList;

    [Header("Color Parameters")]
    public Color currentColor;
    public Image colorImage;

    [Header("Patterns")]
    public List<Vector2> patternCross;



    void Start()
    {
        CheckResolution();
    }


    void Update()
    {

    }

    private void CheckResolution()
    {
        //este evento se encarga de calcular las dimensiones del lienzo
        //A partir de ellas, calcula cuántos BEADS caben en X Y

        //Estas dos variables guardan la resolución total del canvas
        float sizeX = mainCanvas.sizeDelta.x; //Ancho total
        float sizeY = mainCanvas.sizeDelta.y; //Alto total

        //Estas cuatro variables guardan la resolución total del Panel
        float boardLeft = mainPanel.offsetMin.x; //margen izquierdo
        float boardRight = mainPanel.offsetMax.x; //margen derecho
        float boardUp = mainPanel.offsetMin.y; //margen superior
        float boardDown = mainPanel.offsetMax.y; //margen inferior

        //Con estos datos ya puedo calcular cuánta medida útil tengo en X Y

        float totalBoardX = sizeX - (boardLeft + Mathf.Abs(boardRight));

        float totalBoardY = sizeY - (boardUp + Mathf.Abs(boardDown));

        //Ahora puedo calcular cuántos beads caben en X Y
        int beadsX = (int)(totalBoardX / 20);
        int beadsY = (int)(totalBoardY / 20);

        //-----------------------------------------------------------------------------------------------------------
        //                          INSTANCIAR BEADS

        int totalBeads = beadsX * beadsY; //calculamos el total de BEADS

        //el FOR repite ESTE VVVVVVVVVV número de veces lo que ponga entre las llaves
        for (int i = 0; i < totalBeads; i++)
        {
            //Instanciamos un BEAD
            GameObject newBead = Instantiate(bead, boardParent);

            //de ese BEAD guardamos su componente de Image
            Image beadImage = newBead.GetComponent<Image>();
            beadList.Add(beadImage);


            //A ese BEAD le incluyo un componente de Event Trigger
            EventTrigger newTrigger = newBead.AddComponent<EventTrigger>();




            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            //Especifico qué es un Pointer Down
            pointerDown.eventID = EventTriggerType.PointerDown;
            //Asigno la consecuencia de dicho Pointer Down
            pointerDown.callback.AddListener(delegate { PaintBead(beadImage); });
            //Incorporamos esta función al componente del BEAD
            newTrigger.triggers.Add(pointerDown);


            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener(delegate { PaintBead(beadImage); });

            newTrigger.triggers.Add(pointerEnter);



        }
    }
    public void PaintBead(Image currentBead)
    {
        if (Input.GetMouseButton(0))
        {
            //Antes de nada, compruebo el modo en el que estoy
            //SWITCH
            switch (currentMode)
            {
                //Dentro de estas llaves, pondremos la consecuencia de cada uno de los estados
                case InteractionMode.Paint:
                    //Entre case y el break se pone lo que sucede
                    currentBead.color = currentColor;

                    break;

                case InteractionMode.Erase: //Lo que pasa cuando estoy en modo borrar
                    currentBead.color = Color.white;

                    break;

                case InteractionMode.Fill: //Lo que pasa cuando estoy en modo Bote de Pintura
                    //Busca todos los beads (todos los hijos del board) del mismo color y los pinta del color 
                    
                        //uso del GetComponent. Muy recomendable siempre que se vaya a REPETIR.
                        for (int i = 0; i < beadList.Count; i++)
                        {
                            if(beadList[i] != currentBead)
                            {
                                if (beadList[i].color == currentBead.color)
                                {
                                    beadList[i].color = currentColor;
                                }
                            }
                            
                        }

                    currentBead.color = currentColor;
                    break;

                case InteractionMode.Pattern:
                    //Dentro de este case, habrá varias posibles situaciones, por lo que en lugar
                    //de hacer siempre una misma acción, creo otro siwtch dentro.
                    switch(currentPattern)
                    {
                        case PatternEnum.Horizontal:
                            float beadposY = currentBead.gameObject.transform.position.y;
                            //busco todos los beads con la misma posición Y que yo y los pinto
                            for (int i = 0; i < beadList.Count; i++)
                            {
                                if (beadList[i].gameObject.transform.position.y == beadposY)
                                    
                                {
                                    beadList[i].color = currentColor;
                                }
                            }
                            break;

                        case PatternEnum.Vertical:
                            float beadposX = currentBead.gameObject.transform.position.x;
                            //busco todos los beads con la misma posición Y que yo y los pinto
                            for (int i = 0; i < beadList.Count; i++)
                            {
                                if (beadList[i].gameObject.transform.position.x == beadposX)

                                {
                                    beadList[i].color = currentColor;
                                }
                            }
                            break;


                        case PatternEnum.Cross:
                            for (int i = 0; i < patternCross.Count; i++)
                            {
                                //Busco un bead con esa posición y lo pinto
                                Vector2 convertedPos = new Vector2(
                                    (int)currentBead.transform.localPosition.x + patternCross[i].x * 20,
                                    (int)currentBead.transform.localPosition.y + patternCross[i].y * 20);
                                

                                PaintSpecificBead(convertedPos);
                            }
                            break;

                    }
                    break;

                case InteractionMode.Pick: //Lo que pasa en modo Cuentoagotas

                    currentColor = currentBead.color;
                    UpdateColor();
                    ChangeTool(0);
                    break;
            }
           
        }

    }

    //Este método busca un BEAD en una posición específica y lo pinta
    private void PaintSpecificBead(Vector2 targetPos)
    {
        //Recorro la lista de BEADS
        for (int i = 0; i < beadList.Count; i++)
        {

            Vector2 roundedPosition = new Vector2(
                (int)beadList[i].transform.localPosition.x,
                (int)beadList[i].transform.localPosition.y);

            //Si alguno de ellos tiene la misma POS que he enviado al llamar a este estado
            if(roundedPosition == targetPos)
            {
                //Pinto ese BEAD
                beadList[i].color = currentColor;
            }
        }
    }
    
    public void ChangeTool(int ToolIndex)
    {
        currentMode = (InteractionMode)ToolIndex;
        //Ahora se puede ver la herramienta que se usa a traves de una imagen

        //Accedemos a todos los hijos del ToolParent
        for (int i = 0; i < ToolParent.childCount; i++)
        {
            // Y COMPROBAMOS SI SU INDICE DE HIJO ES IGUAL AL DE LA HERRAMIENTA ACTUAL
            ToolParent.GetChild(i).gameObject.SetActive(i == ToolIndex);
        }
    }
    
    public void ChangePattern(int patternIndex)
    {
        currentPattern = (PatternEnum)patternIndex;
    }
    public void SetColor(Image newColor)
    {
        currentColor = newColor.color;
        UpdateColor();
    }
    private void UpdateColor()
    {
        //Este evento actualiza el color de imagen colorIamge
        colorImage.color = currentColor;
    }
}

