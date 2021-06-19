using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownParser {
    public class Grammar {

        private List<NonTerminal> NonTerminals { get; } = new();
        public NonTerminal Start { get; set; } = null;

        public NonTerminal CreateNonTerminal() {
            var nt = new NonTerminal();
            NonTerminals.Add(nt);
            Start = Start ?? nt;
            return nt;
        }

        internal string Input { get; private set; }
        internal Stack<(NonTerminal nt, bool consumed)> Stack { get; private set; }

        //public bool Accept()

        public bool Parse(string value) {
            if (Start == null) {
                throw new InvalidOperationException("Start symbol is set tu null");
            }

            Input = value;
            Stack = new();

            Stack.Push((Start, false));

            return Start.TryParse(this);
        }
    }

    public abstract class GrammarExpression {

        public static GrammarExpression operator |(GrammarExpression e1, GrammarExpression e2)
            => e1.Or(e2);
        public static GrammarExpression operator |(string e1, GrammarExpression e2)
            => ((Terminal)e1).Or(e2);
        public static GrammarExpression operator |(GrammarExpression e1, string e2)
            => e1.Or((Terminal)e2);

        public static GrammarExpression operator &(GrammarExpression e1, GrammarExpression e2)
            => e1.Concat(e2);
        public static GrammarExpression operator &(string e1, GrammarExpression e2)
            => ((Terminal)e1).Concat(e2);
        public static GrammarExpression operator &(GrammarExpression e1, string e2)
            => e1.Concat((Terminal)e2);

        internal virtual GrammarOrExpression Or(GrammarExpression other) {
            return new GrammarOrExpression(new[] { this, other });
        }
        internal virtual GrammarConcatExpression Concat(GrammarExpression other) {
            return new GrammarConcatExpression(new[] { this, other })
        }

        public virtual bool TryParse(Grammar g) {
            return false;
        }
    }

    internal class GrammarConcatExpression : GrammarExpression {
        internal List<GrammarExpression> Exprs { get; } = new();
        public GrammarConcatExpression() { }
        public GrammarConcatExpression(IEnumerable<GrammarExpression> exprs) {
            Exprs.AddRange(exprs);
        }
        internal override GrammarConcatExpression Concat(GrammarExpression other) {
            Exprs.Add(other);
            return this;
        }

        public override bool TryParse(Grammar g) {
            foreach (var e in Exprs) {
                if (!e.TryParse(g)) {
                    return false;
                }
            }

            return true;
        }
    }

    internal class GrammarOrExpression : GrammarExpression {

        internal List<GrammarExpression> Exprs { get; } = new();
        public GrammarOrExpression() { }
        public GrammarOrExpression(IEnumerable<GrammarExpression> exprs) {
            Exprs.AddRange(exprs);
        }

        internal override GrammarOrExpression Or(GrammarExpression other) {
            Exprs.Add(other);
            return this;
        }

        public override bool TryParse(Grammar g) {
            foreach (var e in Exprs) {
                if (e.TryParse(g)) {
                    return true;
                }
            }

            return false;
        }
    }

    internal class Terminal : GrammarExpression {
        public string Value { get; }

        public static readonly Terminal Lambda = new Terminal("");

        public static implicit operator Terminal(string value)
            => (value == Lambda.Value)
                ? Lambda
                : new Terminal(value);


        private Terminal(string value) {
            Value = value;
        }

        public override bool TryParse(Grammar g) {
            return base.TryParse(g);
        }
    }
    public class NonTerminal : GrammarExpression {

        internal GrammarOrExpression Rule { get; private set; }
        public static NonTerminal operator +(NonTerminal nt, GrammarExpression expr) {
            if (expr is GrammarOrExpression or) {
                nt.Rule = or;
            } else {
                nt.Rule = new GrammarOrExpression(new[] { expr });
            }
            return nt;
        }

    }
}
