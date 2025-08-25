using System;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace GameDevUtils
{
    /// <summary>
    /// Collection of high-performance math helpers frequently used in game development.
    /// </summary>
    public static class MathFast
    {
        /// <summary>
        /// Computes an approximation of <c>1 / sqrt(x)</c> using the Quake III fast inverse square root algorithm.
        /// </summary>
        /// <param name="x">Value to invert.</param>
        /// <returns>Approximate reciprocal square root of <paramref name="x"/>.</returns>
        /// <example>
        /// <code>
        /// float inv = MathFast.FastInvSqrt(25f); // ≈ 0.2f
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FastInvSqrt(float x)
        {
            unsafe
            {
                float xhalf = 0.5f * x;
                int i = *(int*)&x;
                i = 0x5f3759df - (i >> 1);
                x = *(float*)&i;
                x = x * (1.5f - xhalf * x * x);
                return x;
            }
        }

        /// <summary>
        /// Determines whether an integer is a power of two.
        /// </summary>
        /// <param name="value">The integer to test.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is a power of two.</returns>
        /// <example>
        /// <code>
        /// bool isPow2 = MathFast.IsPowerOfTwo(64); // true
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;

        /// <summary>
        /// Returns the next power of two greater than or equal to the specified value.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <returns>The next power of two.</returns>
        /// <example>
        /// <code>
        /// int size = MathFast.NextPowerOfTwo(70); // 128
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextPowerOfTwo(int value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        /// <summary>
        /// Restricts a value to the [0,1] range without branching.
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <returns>Value clamped between 0 and 1.</returns>
        /// <example>
        /// <code>
        /// float t = MathFast.Clamp01(1.5f); // 1
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

        /// <summary>
        /// Wraps an angle in radians to the (-π, π] range.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>Wrapped angle.</returns>
        /// <example>
        /// <code>
        /// float wrapped = MathFast.WrapAngle(7.0f); // ≈ 0.7168f
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float WrapAngle(float angle)
        {
            const float twoPi = MathF.PI * 2f;
            angle %= twoPi;
            if (angle <= -MathF.PI) angle += twoPi;
            else if (angle > MathF.PI) angle -= twoPi;
            return angle;
        }
    }

    /// <summary>
    /// Minimal high-performance object pool.
    /// </summary>
    /// <typeparam name="T">Reference type to pool.</typeparam>
    /// <example>
    /// <code>
    /// var pool = new ObjectPool<MyClass>(() => new MyClass());
    /// MyClass item = pool.Rent();
    /// // ... use item ...
    /// pool.Return(item);
    /// </code>
    /// </example>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly ConcurrentBag<T> _items = new();
        private readonly Func<T> _factory;

        public ObjectPool(Func<T> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        /// <summary>
        /// Retrieves an instance from the pool or creates a new one if the pool is empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Rent() => _items.TryTake(out var item) ? item : _factory();

        /// <summary>
        /// Returns an instance to the pool.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            if (item != null)
                _items.Add(item);
        }
    }
}
