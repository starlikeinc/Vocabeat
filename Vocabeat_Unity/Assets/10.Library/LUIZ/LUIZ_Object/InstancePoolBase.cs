using System.Collections;
using System.Collections.Generic;

namespace LUIZ
{
    //Mono 객체가 아닌 일반적인 object 객체 사용 중 간단한 pooling 이 필요할 경우 해당 클래스를 상속 받아 이용할 수 있음
    //풀에 객체가 많이 쌓이더라도 따로 해제하지 않으므로 반환 없는 다량 호출에 유의가 필요
    public abstract class InstancePoolBase<TInstance> : object where TInstance : InstancePoolBase<TInstance>
    {
        private static Queue<TInstance> c_queInstancePool = new Queue<TInstance>();

        //---------------------------------------------------------------
        /// <summary>
        /// Pool에서 객체를 반환받는다. Pool이 비어있다면 새 객체가 생성된 후 반환된다.
        /// 반환 시 해당 객체의 OnInstancePoolGet()가 호출되며
        /// 새로 생성된 객체일 경우 OnInstancePoolCreation() 또한 호출된다.
        /// </summary>
        public static TGet InstancePoolGet<TGet>() where TGet : TInstance, new()
        {
            TGet newInstance = PrivGetInstanceFromPool<TGet>();
            newInstance.OnInstancePoolGet();
            return newInstance;
        }

        public static void InstancePoolReturn(TInstance returnInstance)
        {
            c_queInstancePool.Enqueue(returnInstance);
            returnInstance.OnInstancePoolReturn();
        }

        //---------------------------------------------------------------
        private static TGet PrivGetInstanceFromPool<TGet>() where TGet : TInstance, new()
        {
            TGet getInstance = null;

            if (c_queInstancePool.Count > 0)
            {
                getInstance = c_queInstancePool.Dequeue() as TGet;
            }
            else
            {
                getInstance = new TGet();
                getInstance.OnInstancePoolCreation();
            }

            return getInstance;
        }

        //---------------------------------------------------------------
        protected virtual void OnInstancePoolCreation() { }
        protected virtual void OnInstancePoolGet() { }
        protected virtual void OnInstancePoolReturn() { }
    }
}
