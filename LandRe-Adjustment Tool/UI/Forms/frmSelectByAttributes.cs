using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Land_Readjustment_Tool.UI.CustomControls;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmSelectByAttributes : Form
    {
        private readonly List<SelectionAttributeLayer> _allLayers;

        public event Func<IReadOnlyList<Guid>, CanvasSelectionApplyMode, bool, int>? SelectionRequested;

        public frmSelectByAttributes(IEnumerable<SelectionAttributeLayer> layers)
        {
            InitializeComponent();

            _allLayers = layers
                .OrderBy(layer => layer.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            cboMethod.SelectedIndex = 0;
            cboLayer.SelectedIndexChanged += (_, _) => RefreshLayerFieldsAndValues();
            chkOnlySelectableLayers.CheckedChanged += (_, _) => PopulateLayers();
            lstFields.DoubleClick += (_, _) => InsertSelectedField();
            lstFields.SelectedIndexChanged += (_, _) => ClearUniqueValues();
            lstValues.DoubleClick += (_, _) => InsertSelectedValue();
            btnGetUniqueValues.Click += (_, _) => PopulateUniqueValues();
            txtGoTo.TextChanged += (_, _) => SelectFirstMatchingValue();
            btnClear.Click += (_, _) => txtExpression.Clear();
            btnVerify.Click += (_, _) => VerifyExpression(showSuccess: true);
            btnApply.Click += (_, _) => ApplySelection(closeAfterApply: false);
            btnOk.Click += (_, _) => ApplySelection(closeAfterApply: true);
            btnClose.Click += (_, _) => Close();
            btnLoad.Click += (_, _) => LoadExpression();
            btnSave.Click += (_, _) => SaveExpression();

            WireOperatorButtons();
            PopulateLayers();
        }

        private SelectionAttributeLayer? SelectedLayer => cboLayer.SelectedItem as SelectionAttributeLayer;
        public bool ZoomToSelection => chkZoomToSelection.Checked;

        private void PopulateLayers()
        {
            SelectionAttributeLayer? previous = SelectedLayer;
            cboLayer.BeginUpdate();
            try
            {
                cboLayer.Items.Clear();
                foreach (SelectionAttributeLayer layer in _allLayers.Where(layer =>
                             !chkOnlySelectableLayers.Checked || layer.IsSelectable))
                {
                    cboLayer.Items.Add(layer);
                }
            }
            finally
            {
                cboLayer.EndUpdate();
            }

            if (previous != null)
            {
                for (int i = 0; i < cboLayer.Items.Count; i++)
                {
                    if (cboLayer.Items[i] is SelectionAttributeLayer layer &&
                        layer.LayerId == previous.LayerId &&
                        string.Equals(layer.Name, previous.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        cboLayer.SelectedIndex = i;
                        return;
                    }
                }
            }

            cboLayer.SelectedIndex = cboLayer.Items.Count > 0 ? 0 : -1;
            RefreshLayerFieldsAndValues();
        }

        private void RefreshLayerFieldsAndValues()
        {
            SelectionAttributeLayer? layer = SelectedLayer;
            lblExpression.Text = layer == null
                ? "SELECT * FROM <layer> WHERE:"
                : $"SELECT * FROM {layer.Name} WHERE:";

            lstFields.BeginUpdate();
            try
            {
                lstFields.Items.Clear();
                if (layer != null)
                {
                    foreach (FieldListItem field in GetLayerFieldItems(layer))
                        lstFields.Items.Add(field);
                }
            }
            finally
            {
                lstFields.EndUpdate();
            }

            ClearUniqueValues();
        }

        private void ClearUniqueValues()
        {
            lstValues.Items.Clear();
        }

        private static IReadOnlyList<FieldListItem> GetLayerFieldItems(SelectionAttributeLayer layer)
        {
            Dictionary<string, string> labels = new(StringComparer.OrdinalIgnoreCase);
            foreach (SelectionAttributeRow row in layer.Rows)
            {
                foreach (KeyValuePair<string, string> label in row.Labels)
                    labels.TryAdd(label.Key, label.Value);
                foreach (string key in row.Values.Keys)
                    labels.TryAdd(key, key);
            }

            return labels
                .OrderBy(pair => pair.Value, StringComparer.OrdinalIgnoreCase)
                .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(pair => new FieldListItem(pair.Key, pair.Value))
                .ToList();
        }

        private void PopulateUniqueValues()
        {
            lstValues.BeginUpdate();
            try
            {
                lstValues.Items.Clear();
                if (SelectedLayer == null || lstFields.SelectedItem is not FieldListItem field)
                    return;

                foreach (ValueListItem value in SelectedLayer.Rows
                             .Select(row => row.Values.TryGetValue(field.Key, out object? value) ? value : null)
                             .Where(value => !IsNullOrEmptyValue(value))
                             .Select(value => new ValueListItem(value))
                             .DistinctBy(value => value.SortKey, StringComparer.OrdinalIgnoreCase)
                             .OrderBy(value => value.IsNumeric ? 0 : 1)
                             .ThenBy(value => value.NumericValue)
                             .ThenBy(value => value.SortKey, StringComparer.OrdinalIgnoreCase)
                             .Take(1000))
                {
                    lstValues.Items.Add(value);
                }
            }
            finally
            {
                lstValues.EndUpdate();
            }
        }

        private void SelectFirstMatchingValue()
        {
            string text = txtGoTo.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            for (int i = 0; i < lstValues.Items.Count; i++)
            {
                if (lstValues.Items[i]?.ToString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
                {
                    lstValues.SelectedIndex = i;
                    return;
                }
            }
        }

        private void WireOperatorButtons()
        {
            btnOpEq.Click += (_, _) => InsertExpressionText(" = ");
            btnOpNeq.Click += (_, _) => InsertExpressionText(" <> ");
            btnOpLike.Click += (_, _) => InsertExpressionText(" LIKE ");
            btnOpGt.Click += (_, _) => InsertExpressionText(" > ");
            btnOpGte.Click += (_, _) => InsertExpressionText(" >= ");
            btnOpAnd.Click += (_, _) => InsertExpressionText(" AND ");
            btnOpLt.Click += (_, _) => InsertExpressionText(" < ");
            btnOpLte.Click += (_, _) => InsertExpressionText(" <= ");
            btnOpOr.Click += (_, _) => InsertExpressionText(" OR ");
            btnOpPercent.Click += (_, _) => InsertExpressionText("%");
            btnOpUnderscore.Click += (_, _) => InsertExpressionText("_");
            btnOpParens.Click += (_, _) => InsertExpressionText("(  )", -2);
            btnOpIs.Click += (_, _) => InsertExpressionText(" IS ");
            btnOpIn.Click += (_, _) => InsertExpressionText(" IN ");
            btnOpNot.Click += (_, _) => InsertExpressionText(" NOT ");
        }

        private void InsertSelectedField()
        {
            if (lstFields.SelectedItem is FieldListItem field)
                InsertExpressionText($"\"{field.Key}\"");
        }

        private void InsertSelectedValue()
        {
            if (lstValues.SelectedItem is ValueListItem value)
                InsertExpressionText(value.ExpressionText);
        }

        private void InsertExpressionText(string text, int caretOffset = 0)
        {
            int start = txtExpression.SelectionStart;
            txtExpression.SelectedText = text;
            txtExpression.Focus();
            txtExpression.SelectionStart = Math.Max(0, start + text.Length + caretOffset);
        }

        private bool VerifyExpression(bool showSuccess)
        {
            string expression = txtExpression.Text.Trim();
            if (string.IsNullOrWhiteSpace(expression))
            {
                if (showSuccess)
                {
                    MessageBox.Show(this, "Empty expression is valid and selects all rows in the chosen layer.",
                        "Verify", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }

            try
            {
                SqlExpressionEvaluator.Parse(expression);
                if (showSuccess)
                {
                    MessageBox.Show(this, "The expression is valid.",
                        "Verify", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Invalid Expression", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void ApplySelection(bool closeAfterApply)
        {
            SelectionAttributeLayer? layer = SelectedLayer;
            if (layer == null)
            {
                MessageBox.Show(this, "Select a layer first.", "Select By Attributes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CanvasSelectionApplyMode mode = GetSelectionMode();
            Guid[] ids;
            if (mode == CanvasSelectionApplyMode.Clear)
            {
                ids = [];
            }
            else if (mode == CanvasSelectionApplyMode.Switch && string.IsNullOrWhiteSpace(txtExpression.Text))
            {
                ids = layer.Rows.Select(row => row.CanvasObjectId).ToArray();
            }
            else
            {
                if (!TryEvaluateRows(layer, out ids))
                    return;
            }

            int selectedCount = SelectionRequested?.Invoke(ids, mode, ZoomToSelection && ids.Length > 0) ?? 0;
            UpdateApplyStatus(ids.Length, selectedCount, mode);
            if (closeAfterApply)
                Close();
        }

        private void UpdateApplyStatus(int matchedCount, int selectedCount, CanvasSelectionApplyMode mode)
        {
            string action = mode switch
            {
                CanvasSelectionApplyMode.Add => "added to selection",
                CanvasSelectionApplyMode.Remove => "removed from selection",
                CanvasSelectionApplyMode.Subset => "kept from current selection",
                CanvasSelectionApplyMode.Switch => "switched in selection",
                CanvasSelectionApplyMode.Clear => "cleared",
                _ => "selected"
            };

            lblApplyStatus.Text = mode == CanvasSelectionApplyMode.Clear
                ? "Selection cleared."
                : $"{matchedCount:N0} matched, {selectedCount:N0} currently selected ({action}).";
        }

        private bool TryEvaluateRows(SelectionAttributeLayer layer, out Guid[] ids)
        {
            ids = [];
            string expression = txtExpression.Text.Trim();
            if (string.IsNullOrWhiteSpace(expression))
            {
                ids = layer.Rows.Select(row => row.CanvasObjectId).ToArray();
                return true;
            }

            try
            {
                SqlExpressionEvaluator evaluator = SqlExpressionEvaluator.Parse(expression);
                ids = layer.Rows
                    .Where(row => evaluator.Evaluate(row.Values))
                    .Select(row => row.CanvasObjectId)
                    .ToArray();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Select By Attributes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private CanvasSelectionApplyMode GetSelectionMode()
        {
            return cboMethod.SelectedIndex switch
            {
                1 => CanvasSelectionApplyMode.Add,
                2 => CanvasSelectionApplyMode.Remove,
                3 => CanvasSelectionApplyMode.Subset,
                4 => CanvasSelectionApplyMode.Switch,
                5 => CanvasSelectionApplyMode.Clear,
                _ => CanvasSelectionApplyMode.Create
            };
        }

        private void LoadExpression()
        {
            using OpenFileDialog dialog = new()
            {
                Title = "Load Selection Expression",
                Filter = "Query files (*.sql;*.txt)|*.sql;*.txt|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                txtExpression.Text = System.IO.File.ReadAllText(dialog.FileName);
        }

        private void SaveExpression()
        {
            using SaveFileDialog dialog = new()
            {
                Title = "Save Selection Expression",
                Filter = "SQL files (*.sql)|*.sql|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "selection-query.sql"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                System.IO.File.WriteAllText(dialog.FileName, txtExpression.Text);
        }

        private sealed record FieldListItem(string Key, string Label)
        {
            public override string ToString() => $"\"{Label}\"  ({Key})";
        }

        private sealed class ValueListItem
        {
            public ValueListItem(object? value)
            {
                Value = value;
                IsNumeric = TryParseNumber(value, out decimal number);
                NumericValue = IsNumeric ? number : null;
                SortKey = FormatAttributeValue(value);
            }

            public object? Value { get; }
            public bool IsNumeric { get; }
            public decimal? NumericValue { get; }
            public string SortKey { get; }

            public string ExpressionText => IsNumeric
                ? NumericValue!.Value.ToString(CultureInfo.InvariantCulture)
                : $"'{SortKey.Replace("'", "''")}'";

            public override string ToString() => SortKey;
        }

        private sealed class SqlExpressionEvaluator
        {
            private readonly ExprNode _root;

            private SqlExpressionEvaluator(ExprNode root)
            {
                _root = root;
            }

            public static SqlExpressionEvaluator Parse(string expression)
            {
                Parser parser = new(expression);
                return new SqlExpressionEvaluator(parser.Parse());
            }

            public bool Evaluate(IReadOnlyDictionary<string, object?> row)
            {
                return ToBool(_root.Evaluate(row));
            }

            private abstract class ExprNode
            {
                public abstract object? Evaluate(IReadOnlyDictionary<string, object?> row);
            }

            private sealed class LiteralNode(object? value) : ExprNode
            {
                public override object? Evaluate(IReadOnlyDictionary<string, object?> row) => value;
            }

            private sealed class FieldNode(string key) : ExprNode
            {
                public override object? Evaluate(IReadOnlyDictionary<string, object?> row) =>
                    row.TryGetValue(key, out object? value) ? value : null;
            }

            private sealed class NotNode(ExprNode inner) : ExprNode
            {
                public override object Evaluate(IReadOnlyDictionary<string, object?> row) =>
                    !ToBool(inner.Evaluate(row));
            }

            private sealed class BoolNode(ExprNode left, string op, ExprNode right) : ExprNode
            {
                public override object Evaluate(IReadOnlyDictionary<string, object?> row)
                {
                    return op.Equals("AND", StringComparison.OrdinalIgnoreCase)
                        ? ToBool(left.Evaluate(row)) && ToBool(right.Evaluate(row))
                        : ToBool(left.Evaluate(row)) || ToBool(right.Evaluate(row));
                }
            }

            private sealed class CompareNode(ExprNode left, string op, ExprNode right) : ExprNode
            {
                public override object Evaluate(IReadOnlyDictionary<string, object?> row)
                {
                    object? l = left.Evaluate(row);
                    object? r = right.Evaluate(row);
                    if (op.Equals("LIKE", StringComparison.OrdinalIgnoreCase))
                        return Like(ToText(l), ToText(r));

                    int comparison = Compare(l, r);
                    return op switch
                    {
                        "=" => comparison == 0,
                        "<>" => comparison != 0,
                        ">" => comparison > 0,
                        ">=" => comparison >= 0,
                        "<" => comparison < 0,
                        "<=" => comparison <= 0,
                        _ => false
                    };
                }
            }

            private sealed class IsNullNode(ExprNode left, bool isNot) : ExprNode
            {
                public override object Evaluate(IReadOnlyDictionary<string, object?> row)
                {
                    bool isNull = string.IsNullOrWhiteSpace(ToText(left.Evaluate(row)));
                    return isNot ? !isNull : isNull;
                }
            }

            private sealed class InNode(ExprNode left, IReadOnlyList<ExprNode> values, bool isNot) : ExprNode
            {
                public override object Evaluate(IReadOnlyDictionary<string, object?> row)
                {
                    object? l = left.Evaluate(row);
                    bool contains = values.Any(value => Compare(l, value.Evaluate(row)) == 0);
                    return isNot ? !contains : contains;
                }
            }

            private sealed class Parser
            {
                private readonly List<Token> _tokens;
                private int _position;

                public Parser(string expression)
                {
                    _tokens = Tokenize(expression);
                }

                public ExprNode Parse()
                {
                    ExprNode expression = ParseOr();
                    if (Peek().Kind != TokenKind.End)
                        throw Error($"Unexpected token '{Peek().Text}'.");
                    return expression;
                }

                private ExprNode ParseOr()
                {
                    ExprNode left = ParseAnd();
                    while (MatchKeyword("OR"))
                        left = new BoolNode(left, "OR", ParseAnd());
                    return left;
                }

                private ExprNode ParseAnd()
                {
                    ExprNode left = ParseNot();
                    while (MatchKeyword("AND"))
                        left = new BoolNode(left, "AND", ParseNot());
                    return left;
                }

                private ExprNode ParseNot()
                {
                    if (MatchKeyword("NOT"))
                        return new NotNode(ParseNot());
                    return ParseComparison();
                }

                private ExprNode ParseComparison()
                {
                    ExprNode left = ParsePrimary();
                    bool isNot = MatchKeyword("NOT");

                    if (MatchKeyword("IS"))
                    {
                        bool notNull = MatchKeyword("NOT");
                        ExpectKeyword("NULL");
                        return new IsNullNode(left, notNull);
                    }

                    if (MatchKeyword("IN"))
                    {
                        Expect(TokenKind.LeftParen);
                        List<ExprNode> values = [];
                        if (!Match(TokenKind.RightParen))
                        {
                            do
                            {
                                values.Add(ParsePrimary());
                            } while (Match(TokenKind.Comma));
                            Expect(TokenKind.RightParen);
                        }

                        return new InNode(left, values, isNot);
                    }

                    if (isNot)
                    {
                        if (MatchKeyword("LIKE"))
                            return new NotNode(new CompareNode(left, "LIKE", ParsePrimary()));
                        throw Error("Expected IN or LIKE after NOT.");
                    }

                    if (MatchKeyword("LIKE"))
                        return new CompareNode(left, "LIKE", ParsePrimary());

                    if (Peek().Kind == TokenKind.Operator)
                    {
                        string op = Next().Text;
                        return new CompareNode(left, op, ParsePrimary());
                    }

                    return left;
                }

                private ExprNode ParsePrimary()
                {
                    Token token = Next();
                    return token.Kind switch
                    {
                        TokenKind.String => new LiteralNode(token.Text),
                        TokenKind.Number => new LiteralNode(decimal.Parse(token.Text, CultureInfo.InvariantCulture)),
                        TokenKind.Identifier => token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase)
                            ? new LiteralNode(null)
                            : new FieldNode(token.Text),
                        TokenKind.LeftParen => ParseParenthesized(),
                        _ => throw Error($"Unexpected token '{token.Text}'.")
                    };
                }

                private ExprNode ParseParenthesized()
                {
                    ExprNode expression = ParseOr();
                    Expect(TokenKind.RightParen);
                    return expression;
                }

                private bool MatchKeyword(string keyword)
                {
                    if (Peek().Kind == TokenKind.Identifier &&
                        Peek().Text.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        _position++;
                        return true;
                    }

                    return false;
                }

                private void ExpectKeyword(string keyword)
                {
                    if (!MatchKeyword(keyword))
                        throw Error($"Expected {keyword}.");
                }

                private bool Match(TokenKind kind)
                {
                    if (Peek().Kind != kind)
                        return false;
                    _position++;
                    return true;
                }

                private void Expect(TokenKind kind)
                {
                    if (!Match(kind))
                        throw Error($"Expected {kind}.");
                }

                private Token Peek() => _tokens[_position];
                private Token Next() => _tokens[_position++];
                private InvalidOperationException Error(string message) => new($"Invalid expression: {message}");
            }

            private enum TokenKind
            {
                Identifier,
                String,
                Number,
                Operator,
                LeftParen,
                RightParen,
                Comma,
                End
            }

            private sealed record Token(TokenKind Kind, string Text);

            private static List<Token> Tokenize(string expression)
            {
                List<Token> tokens = [];
                int i = 0;
                while (i < expression.Length)
                {
                    char c = expression[i];
                    if (char.IsWhiteSpace(c))
                    {
                        i++;
                        continue;
                    }

                    if (c == '"')
                    {
                        int start = ++i;
                        while (i < expression.Length && expression[i] != '"')
                            i++;
                        if (i >= expression.Length)
                            throw new InvalidOperationException("Invalid expression: Unterminated field name.");
                        tokens.Add(new Token(TokenKind.Identifier, expression[start..i]));
                        i++;
                        continue;
                    }

                    if (c == '\'')
                    {
                        i++;
                        string value = string.Empty;
                        while (i < expression.Length)
                        {
                            if (expression[i] == '\'' && i + 1 < expression.Length && expression[i + 1] == '\'')
                            {
                                value += "'";
                                i += 2;
                                continue;
                            }

                            if (expression[i] == '\'')
                                break;

                            value += expression[i++];
                        }

                        if (i >= expression.Length)
                            throw new InvalidOperationException("Invalid expression: Unterminated string literal.");
                        tokens.Add(new Token(TokenKind.String, value));
                        i++;
                        continue;
                    }

                    if (char.IsDigit(c) ||
                        (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])) ||
                        ((c == '-' || c == '+') && i + 1 < expression.Length &&
                         (char.IsDigit(expression[i + 1]) ||
                          (expression[i + 1] == '.' && i + 2 < expression.Length && char.IsDigit(expression[i + 2])))))
                    {
                        int start = i++;
                        while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                            i++;
                        tokens.Add(new Token(TokenKind.Number, expression[start..i]));
                        continue;
                    }

                    if (c == '(')
                    {
                        tokens.Add(new Token(TokenKind.LeftParen, "("));
                        i++;
                        continue;
                    }

                    if (c == ')')
                    {
                        tokens.Add(new Token(TokenKind.RightParen, ")"));
                        i++;
                        continue;
                    }

                    if (c == ',')
                    {
                        tokens.Add(new Token(TokenKind.Comma, ","));
                        i++;
                        continue;
                    }

                    if ("=<>".Contains(c))
                    {
                        string op = c.ToString();
                        if (i + 1 < expression.Length &&
                            ((c is '<' or '>' && expression[i + 1] == '=') ||
                             (c == '<' && expression[i + 1] == '>')))
                        {
                            op += expression[i + 1];
                            i++;
                        }
                        tokens.Add(new Token(TokenKind.Operator, op));
                        i++;
                        continue;
                    }

                    if (char.IsLetter(c) || c == '_' || c == '.')
                    {
                        int start = i++;
                        while (i < expression.Length &&
                               (char.IsLetterOrDigit(expression[i]) || expression[i] is '_' or '.'))
                        {
                            i++;
                        }
                        tokens.Add(new Token(TokenKind.Identifier, expression[start..i]));
                        continue;
                    }

                    throw new InvalidOperationException($"Invalid expression: Unexpected character '{c}'.");
                }

                tokens.Add(new Token(TokenKind.End, string.Empty));
                return tokens;
            }
        }

        private static bool ToBool(object? value)
        {
            return value switch
            {
                bool boolean => boolean,
                null => false,
                string text => !string.IsNullOrWhiteSpace(text),
                _ => true
            };
        }

        private static string ToText(object? value) => FormatAttributeValue(value);

        private static bool Like(string value, string pattern)
        {
            string regex = "^" + Regex.Escape(pattern)
                .Replace("%", ".*")
                .Replace("_", ".") + "$";
            return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase);
        }

        private static int Compare(object? left, object? right)
        {
            if (TryParseNumber(left, out decimal leftNumber) &&
                TryParseNumber(right, out decimal rightNumber))
            {
                return leftNumber.CompareTo(rightNumber);
            }

            return string.Compare(ToText(left), ToText(right), StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseNumber(object? value, out decimal number)
        {
            number = 0m;
            if (value is decimal decimalValue)
            {
                number = decimalValue;
                return true;
            }
            if (value is double doubleValue && !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue))
            {
                number = Convert.ToDecimal(doubleValue, CultureInfo.InvariantCulture);
                return true;
            }
            if (value is float floatValue && !float.IsNaN(floatValue) && !float.IsInfinity(floatValue))
            {
                number = Convert.ToDecimal(floatValue, CultureInfo.InvariantCulture);
                return true;
            }
            if (value is int or long or short or byte)
            {
                number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return true;
            }
            if (value is bool)
                return false;

            string text = ToText(value).Trim();
            return decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number) ||
                   decimal.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out number);
        }

        private static bool IsNullOrEmptyValue(object? value)
        {
            return value == null ||
                   value == DBNull.Value ||
                   value is string text && string.IsNullOrWhiteSpace(text);
        }

        private static string FormatAttributeValue(object? value)
        {
            return value switch
            {
                null => string.Empty,
                decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
                double doubleValue => doubleValue.ToString("G15", CultureInfo.InvariantCulture),
                float floatValue => floatValue.ToString("G9", CultureInfo.InvariantCulture),
                DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                bool boolValue => boolValue ? "True" : "False",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}
