using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileCalculator : MonoBehaviour
{
    public TMP_InputField inputA;
    public TMP_InputField inputB;
    public TMP_Dropdown dropdownOperation;
    public TMP_Text resultText;

    public Button btnCalculate;
    public Button btnClear;

    private enum Op { Add, Sub, Mul, Div }
    private Op currentOp = Op.Add;

    void Start()
    {
        if (btnCalculate != null) { btnCalculate.onClick.RemoveAllListeners(); btnCalculate.onClick.AddListener(Calculate); }
        if (btnClear     != null) { btnClear.onClick.RemoveAllListeners();     btnClear.onClick.AddListener(ClearAll); }

        if (dropdownOperation != null)
        {
            dropdownOperation.onValueChanged.RemoveAllListeners();
            if (dropdownOperation.options.Count < 4)
            {
                dropdownOperation.ClearOptions();
                dropdownOperation.AddOptions(new System.Collections.Generic.List<string> { "+", "−", "×", "÷" });
            }
            dropdownOperation.value = 0;
            dropdownOperation.onValueChanged.AddListener(OnDropdownChanged);
            OnDropdownChanged(dropdownOperation.value);
        }

        ShowInfo("Entrer des valeurs avant de calculer.");
    }

    private void OnDropdownChanged(int idx)
    {
        switch (idx)
        {
            case 0: currentOp = Op.Add; break;
            case 1: currentOp = Op.Sub; break;
            case 2: currentOp = Op.Mul; break;
            case 3: currentOp = Op.Div; break;
            default: currentOp = Op.Add; break;
        }
    }

    private void SetOp(Op op)
    {
        currentOp = op;
        if (dropdownOperation != null)
        {
            // Garde le dropdown en cohérence visuelle si les deux existent
            dropdownOperation.value = op switch
            {
                Op.Add => 0,
                Op.Sub => 1,
                Op.Mul => 2,
                Op.Div => 3,
                _ => 0
            };
        }
    }

    public void Calculate()
    {
        if (resultText == null) return;

        if (!TryParseNumber(inputA, out double a, out string errA))
        {
            ShowError($"Entrée A invalide : {errA}");
            return;
        }
        if (!TryParseNumber(inputB, out double b, out string errB))
        {
            ShowError($"Entrée B invalide : {errB}");
            return;
        }

        try
        {
            double res = 0;
            switch (currentOp)
            {
                case Op.Add: res = a + b; break;
                case Op.Sub: res = a - b; break;
                case Op.Mul: res = a * b; break;
                case Op.Div:
                    if (Math.Abs(b) < double.Epsilon)
                    {
                        ShowError("Erreur : division par zéro");
                        return;
                    }
                    res = a / b;
                    break;
            }

            // Affichage résultat (Invariant pour éviter les surprises “,” vs “.”)
            ShowSuccess(res.ToString("G15", CultureInfo.InvariantCulture));
            Debug.Log($"[CALC] {a} {OpToSymbol(currentOp)} {b} = {res}");
        }
        catch (Exception ex)
        {
            ShowError("Erreur de calcul");
            Debug.LogError($"[CALC][ERROR] {ex}");
        }
    }

    public void ClearAll()
    {
        if (inputA != null) inputA.SetTextWithoutNotify("");
        if (inputB != null) inputB.SetTextWithoutNotify("");
        ShowInfo("Prêt.");
    }

    private bool TryParseNumber(TMP_InputField field, out double value, out string error)
    {
        value = 0; error = null;
        if (field == null) { error = "champ manquant"; return false; }

        var raw = (field.text ?? "").Trim();
        if (string.IsNullOrEmpty(raw)) { error = "vide"; return false; }

        // Autoriser “,” ou “.” pour mobile FR
        var norm = raw.Replace(',', '.');

        // Essai Invariant (point)
        if (double.TryParse(norm, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        // Essai culture locale (au cas où)
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return true;

        error = $"“{raw}”";
        return false;
    }

    private string OpToSymbol(Op op) => op switch
    {
        Op.Add => "+",
        Op.Sub => "−",
        Op.Mul => "×",
        Op.Div => "÷",
        _ => "?"
    };

    private void ShowError(string msg)
    {
        if (resultText == null) return;
        resultText.text = msg;
        resultText.color = new Color(0.85f, 0.2f, 0.2f);
    }

    private void ShowSuccess(string msg)
    {
        if (resultText == null) return;
        resultText.text = msg;
        resultText.color = new Color(0.2f, 0.8f, 0.3f);
    }

    private void ShowInfo(string msg)
    {
        if (resultText == null) return;
        resultText.text = msg;
        resultText.color = new Color(1f, 1f, 1f, 0f);
    }
}
