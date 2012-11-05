using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace KendoGridCrudController
{
	public interface IViewModel<TEntity> where TEntity : EntityObject
	{
		void CopyModel(TEntity model);
		void UpdateModel(TEntity model);
	}
}
