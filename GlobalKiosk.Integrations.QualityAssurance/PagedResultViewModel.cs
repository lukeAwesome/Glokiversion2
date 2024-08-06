using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalKiosk.Integrations.QualityAssurance
{
	using System.Collections.Generic;

	using GlobalKiosk.Api.Internal.PagedQueries;
	using Newtonsoft.Json;

	public class PagedResultViewModel<T>
	{

		[JsonConstructor]
		public PagedResultViewModel(QueryPagingInfo paging, List<T> items, string error)
		{
			this.Error = error;
			this.Items = items;
			this.Paging = paging;
		}

		public QueryPagingInfo Paging { get; set; }
		public List<T> Items { get; }
		public string Error { get; set; }

		public PagedResultViewModel()
		{
		}

	}
}
