using System;

namespace Common.Grid.Path
{
    public enum GridPathDataResponse
    {
        InvalidPosition,
        OutOfDataRange,
        Success
    }

    public interface IGridPathDataProvider<TTile, TTerrain, TPosition, TContext>
        where TTile : GridTile<TTerrain, TPosition, TContext>
        where TTerrain : GridTerrain<TContext>
    {
        IGridPathData<TTile, TTerrain, TPosition, TContext> GetGridPathData(TPosition i_Min, TPosition i_Max);
    }

    public interface IGridPathData<TTile, TTerrain, TPosition, TContext> : IDisposable
        where TTile : GridTile<TTerrain, TPosition, TContext>
        where TTerrain : GridTerrain<TContext>
    {
        GridPathDataResponse TryGetElement(TPosition i_Pos, out GridPathElement<TTile, TTerrain, TPosition, TContext> o_Value);

        void Grow(TPosition i_EnvelopPos);
    }
}
