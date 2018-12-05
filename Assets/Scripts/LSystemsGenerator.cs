///\\\==========================================///\\\
///\\\ Title    -   LSystemsGenerator.cs        ///\\\
///\\\ Author   -   Peter Phillips              ///\\\
///\\\ Date     -   First entry:    08.10.18    ///\\\
///\\\              Lastentry:      04.12.18    ///\\\
///\\\==========================================///\\\


using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;

public class LSystemsGenerator : MonoBehaviour
{
    public static int NUM_OF_TREES = 8;
    public static int MAX_ITERATIONS = 7;

    public int title = 1;
    public int iterations = 4;
    public float angle = 30f;
    public float width = 1f;
    public float length = 2f;
    public float variance = 10f;
    public bool hasTreeChanged = false;
    public GameObject Tree = null;

    [SerializeField] private GameObject treeParent;
    [SerializeField] private GameObject branch;
    [SerializeField] private GameObject leaf;
    [SerializeField] private HUDScript HUD;

    private const string axiom = "X";

    private Dictionary<char, string> rules;
    private Stack<TransformInfo> transformStack;
    private int titleLastFrame;
    private int iterationsLastFrame;
    private float angleLastFrame;
    private float widthLastFrame;
    private float lengthLastFrame;
    private string currentString = string.Empty;
    private Vector3 initialPosition = Vector3.zero;
    private float[] randomRotationValues = new float[100];
    
    private void Start()
    {
        titleLastFrame = title;
        iterationsLastFrame = iterations;
        angleLastFrame = angle;
        widthLastFrame = width;
        lengthLastFrame = length;

        for (int i = 0; i < randomRotationValues.Length; i++)
        {
            randomRotationValues[i] = UnityEngine.Random.Range(-1f, 1f);
        }

        transformStack = new Stack<TransformInfo>();

        rules = new Dictionary<char, string>
        {
            { 'X', "[F-[[X]+X]+F[+FX]-X]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void Update()
    {
        if (HUD.hasGenerateBeenPressed || Input.GetKeyDown(KeyCode.G))
        {
            ResetRandomValues();
            HUD.hasGenerateBeenPressed = false;
            Generate();
        }

        if (HUD.hasResetBeenPressed || Input.GetKeyDown(KeyCode.R))
        {
            ResetTreeValues();
            HUD.hasResetBeenPressed = false;
            HUD.Start();
            Generate();
        }

        if (titleLastFrame != title)
        {
            if (title >= 6)
            {
                HUD.rotation.gameObject.SetActive(true);
            }
            else
            {
                HUD.rotation.value = 0;
                HUD.rotation.gameObject.SetActive(false);
            }

            switch (title)
            {
                case 1:
                    SelectTreeOne();
                    break;

                case 2:
                    SelectTreeTwo();
                    break;

                case 3:
                    SelectTreeThree();
                    break;

                case 4:
                    SelectTreeFour();
                    break;

                case 5:
                    SelectTreeFive();
                    break;

                case 6:
                    SelectTreeSix();
                    break;

                case 7:
                    SelectTreeSeven();
                    break;

                case 8:
                    SelectTreeEight();
                    break;

                default:
                    SelectTreeOne();
                    break;
            }

            titleLastFrame = title;
        }

        if (iterationsLastFrame != iterations)
        {
            if (iterations >= 6)
            {
                HUD.warning.gameObject.SetActive(true);
                StopCoroutine("TextFade");
                StartCoroutine("TextFade");
            }
            else
            {
                HUD.warning.gameObject.SetActive(false);
            }
        }

        if (iterationsLastFrame != iterations ||
                angleLastFrame  != angle ||
                widthLastFrame  != width ||
                lengthLastFrame != length)
        {
            ResetFlags();
            Generate();
        }

    }

    private void Generate()
    {
        Destroy(Tree);

        Tree = Instantiate(treeParent);

        currentString = axiom;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < iterations; i++)
        {
            foreach (char c in currentString)
            {
                sb.Append(rules.ContainsKey(c) ? rules[c] : c.ToString());
            }

            currentString = sb.ToString();
            sb = new StringBuilder();
        }

        Debug.Log(currentString);
        
        for (int i = 0; i < currentString.Length; i++)
        {
            switch (currentString[i])
            {
                case 'F':                    
                    initialPosition = transform.position;
                    transform.Translate(Vector3.up * 2 * length);                    

                    GameObject fLine = currentString[(i + 1) % currentString.Length] == 'X' || currentString[(i + 3) % currentString.Length] == 'F' && currentString[(i + 4) % currentString.Length] == 'X' ? Instantiate(leaf) : Instantiate(branch);
                    fLine.transform.SetParent(Tree.transform);
                    fLine.GetComponent<LineRenderer>().SetPosition(0, initialPosition);
                    fLine.GetComponent<LineRenderer>().SetPosition(1, transform.position);
                    fLine.GetComponent<LineRenderer>().startWidth = width;
                    fLine.GetComponent<LineRenderer>().endWidth = width;
                    break;

                case 'X':                
                    break;

                case '+':
                    transform.Rotate(Vector3.back * angle * (1 + variance / 100 * randomRotationValues[i % randomRotationValues.Length]));
                    break;

                case '-':                                      
                    transform.Rotate(Vector3.forward * angle * (1 + variance / 100 * randomRotationValues[i % randomRotationValues.Length]));
                    break;

                case '*':
                    transform.Rotate(Vector3.up * 120 * (1 + variance / 100 * randomRotationValues[i % randomRotationValues.Length]));
                    break;

                case '/':
                    transform.Rotate(Vector3.down* 120 * (1 + variance / 100 * randomRotationValues[i % randomRotationValues.Length]));
                    break;

                case '[':
                    transformStack.Push(new TransformInfo()
                    {
                        position = transform.position,
                        rotation = transform.rotation
                    });
                    break;

                case ']':
                    TransformInfo ti = transformStack.Pop();
                    transform.position = ti.position;
                    transform.rotation = ti.rotation;
                    break;

                default:
                    throw new InvalidOperationException("Invalid L-tree operation");
            }
        }

        Tree.transform.rotation = Quaternion.Euler(0, HUD.rotation.value, 0);
    }

    private void SelectTreeOne()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[F-[X+X]+F[+FX]-X]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeTwo()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[-FX][+FX][FX]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeThree()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[-FX]X[+FX][+F-FX]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeFour()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[FF[+XF-F+FX]--F+F-FX]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeFive()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[FX[+F[-FX]FX][-F-FXFX]]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeSix()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[F[+FX][*+FX][/+FX]]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeSeven()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[*+FX]X[+FX][/+F-FX]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void SelectTreeEight()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "[F[-X+F[+FX]][*-X+F[+FX]][/-X+F[+FX]-X]]" },
            { 'F', "FF" }
        };

        Generate();
    }

    private void ResetRandomValues()
    {
        for (int i = 0; i < randomRotationValues.Length; i++)
        {
            randomRotationValues[i] = UnityEngine.Random.Range(-1f, 1f);
        }
    }

    private void ResetFlags()
    {
        iterationsLastFrame = iterations;
        angleLastFrame = angle;
        widthLastFrame = width;
        lengthLastFrame = length;
    }

    private void ResetTreeValues()
    {
        iterations = 4;
        angle = 30f;
        width = 1f;
        length = 2f;
        variance = 10f;
    }

    IEnumerator TextFade()
    {
        Color c = HUD.warning.color;

        float TOTAL_TIME = 4f;
        float FADE_DURATION = .25f;

        for (float timer = 0f; timer <= TOTAL_TIME; timer += Time.deltaTime)
        {
            if (timer > TOTAL_TIME - FADE_DURATION)
            {
                c.a = (TOTAL_TIME - timer) / FADE_DURATION;
            }
            else if (timer > FADE_DURATION)
            {
                c.a = 1f;
            }
            else
            {
                c.a = timer / FADE_DURATION;
            }

            HUD.warning.color = c;

            yield return null;
        }
    }
}