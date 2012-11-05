using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

/* Example Usage:
 * 1. Specify the generic type parameters when inheriting
 * 2. At minimum:
 *		a. Override EntitySetName to return the name of the EntitySet in the ObjectContext that contains the EntityTypes.
 *		b. Override the Initializer with an expression that maps an EntityObject to an IViewModel. For default behavior, just wire it up to the auto-generated Initializer in ViewModel.cs.
		
	public class ExampleController : ViewModelCrudController<ObjectContextTypeName, EntityTypeName, ViewModelTypeName>
	{
		protected override string EntitySetName
		{
			get { return "EntitySetName"; }
		}

		protected override System.Linq.Expressions.Expression<Func<ProjectWorkerTime, ProjectWorkerTime_ProjectWorkerTimeViewModel>> Initializer
		{
			get { return ViewModelTypeName.Initializer; }
		}
	}
 */
namespace KendoGridCrudController
{
	/// <summary>
	/// Implements CRUD operations on an ObjectContext
	/// </summary>
	/// <typeparam name="TContext">The type of the ObjectContext</typeparam>
	/// <typeparam name="TModel">The type of the Model</typeparam>
	/// <typeparam name="TViewModel">The type of the ViewModel</typeparam>
	public class ViewModelCrudController<TContext, TModel, TViewModel> : Controller
		where TContext : ObjectContext, new()
		where TModel : EntityObject
		where TViewModel : IViewModel<TModel>, new()
	{
		protected virtual string EntitySetName { get { return String.Empty; } }
		protected virtual Expression<Func<TModel, TViewModel>> Initializer { get { return o => new TViewModel(); } }
		protected virtual Expression<Func<TModel, bool>> Where { get { return o => true; } }
	
		#region Context
		TContext _context;
		protected TContext Context
		{
			get
			{
				if (_context == null)
				{
					_context = new TContext();
				}
				return _context;
			}
		}
		#endregion Context

		#region EntitySet
		ObjectSet<TModel> _entitySet;
		protected ObjectSet<TModel> EntitySet
		{
			get
			{
				if (_entitySet == null)
				{
					_entitySet = Context.CreateObjectSet<TModel>(EntitySetName);
				}
				return _entitySet;
			}
		}
		#endregion EntitySet

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (_context != null)
			{
				_context.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion Dispose

		public virtual ActionResult Select([DataSourceRequest] DataSourceRequest request)
		{
			return Json(EntitySet.Where(Where).Select(Initializer).ToDataSourceResult(request));
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Insert([DataSourceRequest] DataSourceRequest request, TViewModel viewModel)
		{
			return InsertMany(request, new[] { viewModel });
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult InsertMany([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<TViewModel> viewModels)
		{
			if (viewModels == null || !ModelState.IsValid)
				return Json(new TViewModel[] { }.ToDataSourceResult(request, ModelState));

			var models = new List<TModel>();
			foreach (var viewModel in viewModels)
			{
				if (viewModel == null) continue;
				var model = EntitySet.CreateObject();
				IAuditable_Insert(viewModel as IAuditableEntity);
				viewModel.UpdateModel(model);
				EntitySet.AddObject(model);
				models.Add(model);
			}
			Context.SaveChanges();
			foreach (var pair in models.Zip(viewModels, (m, vm) => new { Model = m, ViewModel = vm }))
				pair.ViewModel.CopyModel(pair.Model);
			return Json(viewModels.ToDataSourceResult(request, ModelState));
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Update([DataSourceRequest] DataSourceRequest request, TViewModel viewModel)
		{
			return UpdateMany(request, new[] { viewModel });
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult UpdateMany([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<TViewModel> viewModels)
		{
			if (viewModels != null && ModelState.IsValid)
			{
				foreach (var viewModel in viewModels)
				{
					if (viewModel == null) continue;
					var model = EntitySet.CreateObject();
					IAuditable_Update(viewModel as IAuditableEntity);
					viewModel.UpdateModel(model);
					model.EntityKey = Context.CreateEntityKey(EntitySetName, model);
					object value = null;
					if (Context.TryGetObjectByKey(model.EntityKey, out value))
					{
						EntitySet.ApplyCurrentValues(model);
					}
				}
				Context.SaveChanges();
			}

			return Json(ModelState.ToDataSourceResult());
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Delete([DataSourceRequest] DataSourceRequest request, TViewModel viewModel)
		{
			return DeleteMany(request, new[] { viewModel });
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult DeleteMany([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<TViewModel> viewModels)
		{
			if (viewModels != null && ModelState.IsValid)
			{
				foreach (var viewModel in viewModels)
				{
					if (viewModel == null) continue;
					var model = EntitySet.CreateObject();
					viewModel.UpdateModel(model);
					model.EntityKey = Context.CreateEntityKey(EntitySetName, model);
					object value = null;
					if (Context.TryGetObjectByKey(model.EntityKey, out value))
					{
						EntitySet.DeleteObject((TModel)value);
					}
				}
				Context.SaveChanges();
			}

			return Json(ModelState.ToDataSourceResult());
		}

		private void IAuditable_Insert(IAuditableEntity item)
		{
			if (item == null) return;
			Guid user = (Guid)Membership.GetUser().ProviderUserKey;
			item.CreatedBy = user;
			item.CreatedOn = DateTime.Now;
			item.ModifiedBy = user;
			item.ModifiedOn = DateTime.Now;
		}

		private void IAuditable_Update(IAuditableEntity item)
		{
			if (item == null) return;
			Guid user = (Guid)Membership.GetUser().ProviderUserKey;
			item.ModifiedBy = user;
			item.ModifiedOn = DateTime.Now;
		}
	}
}