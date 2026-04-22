namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Calculator engine supporting +, -, *, / with decimal precision.
    /// CalculationStep tracks the running expression shown in the label above the display.
    /// </summary>
    public sealed class CalculatorService
    {
        private decimal _accumulator;
        private decimal _pendingOperand;
        private char? _pendingOperator;
        private bool _startNewOperand;
        private bool _hasResult;

        public string Display { get; private set; } = "0";
        public string CalculationStep { get; private set; } = string.Empty;

        public void Reset()
        {
            _accumulator = 0;
            _pendingOperand = 0;
            _pendingOperator = null;
            _startNewOperand = false;
            _hasResult = false;
            Display = "0";
            CalculationStep = string.Empty;
        }

        /// <summary>Append a digit or the decimal point to the current operand.</summary>
        public void InputDigit(string digit)
        {
            if (_startNewOperand || _hasResult)
            {
                Display = digit == "." ? "0." : digit;
                _startNewOperand = false;
                _hasResult = false;
                UpdateCalculationStep();
                return;
            }

            if (digit == ".")
            {
                if (!Display.Contains('.'))
                    Display += ".";

                UpdateCalculationStep();
                return;
            }

            Display = Display == "0" ? digit : Display + digit;
            UpdateCalculationStep();
        }

        /// <summary>Apply +, -, *, / — evaluates any pending chained operation first.</summary>
        public void ApplyOperator(char op)
        {
            if (decimal.TryParse(Display, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal current))
            {
                if (_pendingOperator.HasValue && !_startNewOperand)
                    Evaluate(current);
                else
                    _accumulator = current;
            }

            _pendingOperator = op;
            _startNewOperand = true;
            _hasResult = false;
            UpdateCalculationStep();
        }

        /// <summary>Compute the final result and freeze CalculationStep as the full expression.</summary>
        public void Equals()
        {
            if (!decimal.TryParse(Display, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal current))
                return;

            if (_pendingOperator.HasValue)
            {
                string left = FormatDecimal(_accumulator);
                string op = ToOperatorSymbol(_pendingOperator.Value);
                string right = FormatDecimal(current);

                _pendingOperand = current;
                Evaluate(current);

                _pendingOperator = null;
                _startNewOperand = true;
                _hasResult = true;

                // Show the full expression that produced the result, e.g. "508 + 16.93 ="
                CalculationStep = $"{left} {op} {right} =";
                return;
            }

            _pendingOperator = null;
            _startNewOperand = true;
            _hasResult = true;
            CalculationStep = $"{Display} =";
        }

        /// <summary>Remove the last character from the display.</summary>
        public void Backspace()
        {
            if (_startNewOperand || _hasResult)
                return;

            Display = Display.Length > 1 ? Display[..^1] : "0";
            if (Display == "-")
                Display = "0";

            UpdateCalculationStep();
        }

        /// <summary>Toggle sign of current display value.</summary>
        public void ToggleSign()
        {
            if (Display.StartsWith('-'))
                Display = Display[1..];
            else if (Display != "0")
                Display = "-" + Display;

            UpdateCalculationStep();
        }

        /// <summary>Returns the current display as a decimal, or null if unparseable.</summary>
        public decimal? GetResult()
        {
            return decimal.TryParse(Display, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : null;
        }

        /// <summary>Paste a validated number string directly into the display.</summary>
        public bool TrySetDisplay(string value)
        {
            if (!decimal.TryParse(value,
                System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal parsed))
            {
                return false;
            }

            Display = FormatDecimal(parsed);
            _startNewOperand = false;
            _hasResult = false;
            UpdateCalculationStep();
            return true;
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private void Evaluate(decimal rightOperand)
        {
            _accumulator = _pendingOperator switch
            {
                '+' => _accumulator + rightOperand,
                '-' => _accumulator - rightOperand,
                '*' => _accumulator * rightOperand,
                '/' => rightOperand != 0 ? _accumulator / rightOperand : 0,
                _ => rightOperand
            };

            Display = FormatDecimal(_accumulator);
        }

        private static string FormatDecimal(decimal value)
        {
            return value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Rebuilds CalculationStep from current state.
        ///
        /// States and what the label shows:
        ///   - Typing first number, no operator yet  →  "508"  (the current display)
        ///   - Operator just pressed, waiting for 2nd →  "508 +"
        ///   - Typing second number                  →  "508 + 16"
        ///   - After = (set in Equals(), not here)   →  "508 + 16 ="  (frozen)
        /// </summary>
        private void UpdateCalculationStep()
        {
            if (_hasResult)
            {
                // Equals() already wrote the frozen expression; don't overwrite it here.
                return;
            }

            if (_pendingOperator.HasValue)
            {
                string left = FormatDecimal(_accumulator);
                string op = ToOperatorSymbol(_pendingOperator.Value);

                // _startNewOperand = true means the user just pressed an operator
                // and hasn't started typing the right operand yet.
                CalculationStep = _startNewOperand
                    ? $"{left} {op}"
                    : $"{left} {op} {Display}";
            }
            else
            {
                // No operator yet — user is typing (or has just typed) the first number.
                // Show it so the label is never blank during normal input.
                CalculationStep = Display == "0" ? string.Empty : Display;
            }
        }

        private static string ToOperatorSymbol(char op) => op switch
        {
            '*' => "×",
            '/' => "÷",
            _ => op.ToString()
        };
    }
}