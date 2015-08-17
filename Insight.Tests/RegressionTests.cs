using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Insight.Database;

namespace Insight.Tests
{
	[TestFixture]
	public class RegressionTests : BaseTest
	{
		#region Git Issue #18
		class Beer
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int? AlcoholPts { get; set; }
		}

		[Test]
		public void TestIssue18()
		{
			using (var connection = ConnectionWithTransaction())
			{
				connection.ExecuteSql("CREATE TABLE Beer18 (id int identity, name varchar(256), alcoholpts int)");
				connection.ExecuteSql(@"
					CREATE PROC InsertBeer18 @id int, @name varchar(256), @alcoholpts [int] AS 
						INSERT INTO Beer18 (Name, AlcoholPts)
						OUTPUT Inserted.Id
						VALUES (@Name, @AlcoholPts)
				");

				Beer b = new Beer() { AlcoholPts = 11 };
				connection.ExecuteScalar<int>("InsertBeer18", b);
				Assert.AreEqual(11, connection.ExecuteScalarSql<int>("SELECT AlcoholPts FROM Beer18"));
			}
		}

		[Test]
		public void Given_a_null_result_When_querying_for_a_scalar_int_Then_the_result_is_not_silently_converted()
		{
			using (var connection = ConnectionWithTransaction())
			{
				connection.ExecuteSql("create table NullsAreWeird (id int identity, nullableint int)");
				connection.ExecuteSql(@"insert into NullsAreWeird (nullableint) values (null)");

				TestDelegate act = () => connection.ExecuteScalarSql<int>("select nullableint from NullsAreWeird");

				Assert.That(act, Throws.Exception);
			}
		}

		[Test]
		public void Given_a_null_result_When_expecting_an_int_Then_the_result_is_not_silently_converted()
		{
			using (var connection = ConnectionWithTransaction())
			{
				connection.ExecuteSql("create table NullsAreWeird (id int identity, nullableint int)");
				connection.ExecuteSql(@"insert into NullsAreWeird (nullableint) values (null)");

				TestDelegate act = () => connection.QuerySql<int>("select nullableint from NullsAreWeird").Single();

				Assert.That(act, Throws.Exception);
			}
		}

		#endregion
	}
}
