namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Stateless calculator engine supporting +, -, *, / with decimal precision.
    /// Follows standard calculator behaviour: operator-then-operand chain evaluation.
    /// </summary>
    public sealed class CalculatorService
    {
        private decimal _accumulator;
        private decimal _pendingOperand;
        private char? _pendingOperator;
        private bool _startNewOperand;
        private bool _hasResult;

        public string Display { get; private set; } = "0";

        public void Reset()
        {
            _accumulator = 0;
            _pendingOperand = 0;
            _pendingOperator = null;
            _startNewOperand = false;
            _hasResult = false;
            Display = "0";
        }

        /// <summary>Append a digit or the decimal point to the current operand.</summary>
        public void InputDigit(string digit)
        {
            if (_startNewOperand || _hasResult)
            {
                Display = digit == "." ? "0." : digit;
                _startNewOperand = false;
                _hasResult = false;
                return;
            }

            if (digit == ".")
            {
                if (!Display.Contains('.'))
                    Display += ".";
                return;
            }

            Display = Display == "0" ? digit : Display + digit;
        }

        /// <summary>Apply +, -, *, / — evaluates any pending operation first.</summary>
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
        }

        /// <summary>Compute the final result.</summary>
        public void Equals()
        {
            if (!decimal.TryParse(Display, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal current))
                return;

            if (_pendingOperator.HasValue)
            {
                _pendingOperand = current;
                Evaluate(current);
            }

            _pendingOperator = null;
            _startNewOperand = true;
            _hasResult = true;
        }

        /// <summary>Remove the last character from the display.</summary>
        public void Backspace()
        {
            if (_startNewOperand || _hasResult)
                return;

            Display = Display.Length > 1 ? Display[..^1] : "0";
            if (Display == "-")
                Display = "0";
        }

        /// <summary>Toggle sign of current display value.</summary>
        public void ToggleSign()
        {
            if (Display.StartsWith('-'))
                Display = Display[1..];
            else if (Display != "0")
                Display = "-" + Display;
        }

        /// <summary>Returns the current display as a decimal, or null if unparseable.</summary>
        public decimal? GetResult()
        {
            return decimal.TryParse(Display, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : null;
        }

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
            // Remove unnecessary trailing zeros but keep at least one digit
            string s = value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture);
            return s;
        }
    }
}
