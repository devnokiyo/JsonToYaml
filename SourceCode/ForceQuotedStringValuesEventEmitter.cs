using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace JsonToYaml
{
    /// <summary>
    /// ダブルコーテーションで囲むためのクラスです。
    /// こちらで公開されているコードを改修しています。
    /// https://dotnetfiddle.net/lTZ8rm
    /// </summary>
    class ForceQuotedStringValuesEventEmitter : ChainedEventEmitter
    {
        private class EmitterState
        {
            private int valuePeriod;
            private int currentIndex;

            public EmitterState(int valuePeriod)
            {
                this.valuePeriod = valuePeriod;
            }

            public bool VisitNext()
            {
                ++currentIndex;
                return (currentIndex % valuePeriod) == 0;
            }
        }

        private readonly Stack<EmitterState> state = new Stack<EmitterState>();

        public ForceQuotedStringValuesEventEmitter(IEventEmitter nextEmitter) : base(nextEmitter)
        {
            state.Push(new EmitterState(1));
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (state.Peek().VisitNext())
            {
                // JSONのnullは""に変換される。
                eventInfo.Style = ScalarStyle.DoubleQuoted;
                //if (eventInfo.Source.Type == typeof(string))
                //{
                //    eventInfo.Style = ScalarStyle.DoubleQuoted;
                //}
            }

            base.Emit(eventInfo, emitter);
        }

        public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
        {
            state.Peek().VisitNext();
            state.Push(new EmitterState(2));
            base.Emit(eventInfo, emitter);
        }

        public override void Emit(MappingEndEventInfo eventInfo, IEmitter emitter)
        {
            state.Pop();
            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            state.Peek().VisitNext();
            state.Push(new EmitterState(1));
            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceEndEventInfo eventInfo, IEmitter emitter)
        {
            state.Pop();
            base.Emit(eventInfo, emitter);
        }
    }
}
