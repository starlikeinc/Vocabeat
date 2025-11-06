using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LUIZ.DataGraph.Editor
{
	public class NodeInspectorViewEditor : VisualElement
	{
		private ScrollView m_scrollView;
		private DataGraphNodeBase m_target;
		private DataGraphNodeBaseEditor m_targetNodeEditor;

		public NodeInspectorViewEditor()
		{
			CreateInspectorHeader(this);

			m_scrollView = new ScrollView(ScrollViewMode.Vertical)
			{
				style =
				{
					paddingLeft = 10,
					paddingTop = 10,
					paddingBottom = 10,
					paddingRight = 10
				}
			}; Add(m_scrollView);
		}

		public void UpdateInspector(List<DataGraphNodeBaseEditor> selectedNodes)
		{
			if (selectedNodes == null)
				return;
			
			if (selectedNodes.Count == 1)
			{
				m_target = selectedNodes[0].Node;
				m_targetNodeEditor = selectedNodes[0];
			}
			else
			{
				m_scrollView.Clear();
				return;
			}

			m_scrollView.Clear();

			if (m_target == null)
			{
				m_scrollView.Add(new Label("No node selected"));
				return;
			}

			Type nodeType = m_target.GetType();
			Type targetType = m_target.GetType();

			var members = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
				.Select(f => (MemberInfo)f)
				.Concat(
					targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						.Where(p => p.CanRead && p.CanWrite)
				);

			foreach (var member in members)
			{
				// [HideInInspectorView] 붙은 건 무조건 제외
				if (member.GetCustomAttribute<HideInGraphInspector>() != null)
					continue;

				// [ShowInNodeBody] 붙은 것도 제외 (노드 본문에서 처리됨)
				if (member.GetCustomAttribute<NodeBodyAttribute>() != null)
					continue;

				Type memberType;
				if (member is FieldInfo field)
				{
					memberType = field.FieldType;
					var section = field.GetCustomAttribute<GraphInspectorHeaderAttribute>();
					if (section != null)
					{
						m_scrollView.Add(CreateSection(section.Header));
					}
				}
				else if (member is PropertyInfo property)
				{
					memberType = property.PropertyType;
					var section = property.GetCustomAttribute<GraphInspectorHeaderAttribute>();
					if (section != null)
					{
						m_scrollView.Add(CreateSection(section.Header));
					}
				}
				else
				{
					continue;
				}

				if (typeof(IList).IsAssignableFrom(memberType))
				{
					m_scrollView.Add(CreateFieldForList(member, m_target));
				}
				else
				{
					m_scrollView.Add(CreateFieldForMember(member));
				}
			}

		}

		private void CreateInspectorHeader(VisualElement parent)
		{
			var header = new Label("Node Inspector")
			{
				//TODO style sheet로 뺄것!!!!!
				style =
				{
					unityTextAlign = TextAnchor.MiddleLeft,
					fontSize = 13,
					color = Color.white,
					backgroundColor = new Color(0.1f, 0.1f, 0.1f),
					paddingTop = 4,
					paddingBottom = 4,
					paddingLeft = 10,
					marginBottom = 4
				}
			};

			parent.Add(header);
		}

		private VisualElement CreateFieldForList(MemberInfo member, object target)
		{
			var container = new VisualElement()
			{
				style = 
				{
					marginBottom = 6,
				}
			};

			var listNameContainer = new VisualElement()
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.SpaceBetween,
					alignItems = Align.Center
				}
			};
			container.Add(listNameContainer);

			listNameContainer.Add(new Label($"List: {member.Name}") { style = {marginLeft = 2}});

			IList list;
			if(member is FieldInfo field)
			{
				list = (IList)field.GetValue(target);
			}
			else if(member is PropertyInfo property)
			{
				list = (IList)(property.GetValue(target));
			}
			else
			{
				container.Add(new Label("List is null"));
				return container;
			}

			var listContainer = new VisualElement()
			{
				style =
				{
					marginLeft = 25,
					marginRight = 10,
				}
			};
			for (int i = 0; i < list.Count; i++)
			{
				int currentIndex = i;

				var elementColumn = new VisualElement
				{
					style =
					{
						flexDirection = FlexDirection.Column,
						justifyContent = Justify.FlexStart,
						backgroundColor = new Color(0.3f, 0.3f, 0.3f),
						marginTop = 4,
						marginBottom = 3,
						paddingLeft = 5,
						paddingRight = 5,
						paddingTop = 5,
						paddingBottom = 5,
						borderBottomLeftRadius = 4,
						borderBottomRightRadius = 4,
						borderTopLeftRadius = 4,
						borderTopRightRadius = 4,
					}
				};

				var elementHeader = new VisualElement
				{
					style =
					{
						flexDirection = FlexDirection.Row,
						justifyContent = Justify.SpaceBetween,
						alignItems = Align.Center
					}
				};

				elementColumn.Add(elementHeader);

				var label = new Label($"Element {currentIndex}")
				{
					style =
					{
						marginRight = 5,
						marginLeft = 2,
						height = 20,
						unityTextAlign = TextAnchor.MiddleLeft
					}
				};
				elementHeader.Add(label);

				var removeButton = new Button(() => RemoveElementFromList(list, currentIndex, member, target))
				{
					text = "Delete",
					style =
					{
						marginLeft = 5,
						height = 20,
						flexShrink = 0,
					}
				};

				elementHeader.Add(removeButton);

				Type elementType = list.GetType().GetGenericArguments().FirstOrDefault();
				VisualElement inputField = CreateInputFieldForType(elementType, list[currentIndex], value =>
				{
					list[currentIndex] = value;
					ApplyChangesToTarget(target, member, list);
					m_target.NotifyNodeValueChangedEvent();
				}, false);
				inputField.style.flexGrow = 1;
				elementColumn.Add(inputField);

				listContainer.Add(elementColumn);
			}

			container.Add(listContainer);

			var addButton = new Button(() => AddElementToList(list, member, target))
			{
				text = "Add Element",
				style =
				{
					//marginBottom = 10,
					//marginTop = 8,
					marginRight = 9,
				}
			};

			listNameContainer.Add(addButton);

			return container;
		}



		private void ApplyChangesToTarget(object target, MemberInfo member, object updatedList)
		{
			if (member is FieldInfo field)
			{
				field.SetValue(target, updatedList);
			}
			else if (member is PropertyInfo property)
			{
				property.SetValue(target, updatedList);
			}
		}

		private void AddElementToList(IList list, MemberInfo member, object target)
		{
			Type elementType = list.GetType().GetGenericArguments().FirstOrDefault();

			if (elementType == null)
			{
				Debug.LogError("Unable to determine list element type.");
				return;
			}

			object newElement = Activator.CreateInstance(elementType);
			list.Add(newElement);

			UpdateMemberValue(member, target, list);
			UpdateInspector(new List<DataGraphNodeBaseEditor> { m_targetNodeEditor });
		}

		private void RemoveElementFromList(IList list, int index, MemberInfo member, object target)
		{
			list.RemoveAt(index);

			UpdateMemberValue(member, target, list);
			UpdateInspector(new List<DataGraphNodeBaseEditor> { m_targetNodeEditor });
		}

		private void UpdateMemberValue(MemberInfo member, object target, object value)
		{
			if (member is FieldInfo field)
			{
				field.SetValue(target, value);
			}
			else if (member is PropertyInfo property && property.CanWrite)
			{
				property.SetValue(target, value);
			}
		}

		private VisualElement CreateFieldForMember(MemberInfo member)
		{
			Type memberType;
			object memberValue;
			Action<object> setValue;

			VisualElement fieldContainer = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					marginBottom = 4
                }
			};

			if (member is FieldInfo field)
			{
				if(field.FieldType == typeof(string))
				{
					fieldContainer.style.flexDirection = FlexDirection.Column;
				}

				memberType = field.FieldType;
				memberValue = field.GetValue(m_target);
				setValue = value => field.SetValue(m_target, value);
			}
			else if(member is PropertyInfo property && property.CanWrite)
			{
				if(property.PropertyType == typeof(string))
				{
					fieldContainer.style.flexDirection = FlexDirection.Column;
				}

				memberType = property.PropertyType;
				memberValue = property.GetValue(m_target);
				setValue = value => property.SetValue(m_target, value);
			}
			else
			{
				return new Label($"Unsupported member type: {member.MemberType}");
			}

			Label label = new Label(member.Name)
			{
				style =
				{
					minWidth = 100,
					unityTextAlign = TextAnchor.MiddleLeft,
					marginRight = 5,
					marginBottom = 3,
					marginLeft = 2,
				}
			};

			fieldContainer.Add(label);

			var inputField = CreateInputFieldForType(memberType, memberValue, value =>
			{
				setValue(value);
				m_target?.NotifyNodeValueChangedEvent();
			});
			inputField.style.flexGrow = 1;

			fieldContainer.Add(inputField);
			
			return fieldContainer;
		}

		private VisualElement CreateInputFieldForType(Type type, object value, Action<object> setValue, bool isForField = true)
		{
			if (type == typeof(string))
			{
				var textField = new TextField { value = value as string ?? string.Empty, multiline = true };
				textField.style.flexGrow = 1;
				textField.style.whiteSpace = WhiteSpace.Normal;
				if(isForField) textField.style.marginRight = 9;
				textField.RegisterValueChangedCallback(evt =>
				{
					setValue(evt.newValue);
				});
				return textField;
			}
			else if (type.IsEnum)
			{
				var enumField = new EnumField((Enum)value);
				if (isForField) enumField.style.marginRight = 9;
				enumField.RegisterValueChangedCallback(evt =>
				{
					setValue(evt.newValue);
				});
				return enumField;
			}
			else if (type == typeof(int))
			{
				var intField = new IntegerField { value = value != null ? (int)value : 0 };
				if (isForField)  intField.style.marginRight = 9;
				intField.RegisterValueChangedCallback(evt => { setValue(evt.newValue); });
				return intField;
			}
			else if (type == typeof(float))
			{
				var floatField = new FloatField { value = value != null ? (float)value : 0f };
				if (isForField) floatField.style.marginRight = 9;
				floatField.RegisterValueChangedCallback(evt => { setValue(evt.newValue); });
				return floatField;
			}
			else if (type == typeof(bool))
			{
				var toggleField = new Toggle { value = value != null && (bool)value };
				toggleField.RegisterValueChangedCallback(evt => { setValue(evt.newValue); });
				return toggleField;
			}
			else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				var objectField = new ObjectField { objectType = type, value = value as UnityEngine.Object };
				objectField.RegisterValueChangedCallback(evt => { setValue(evt.newValue); });
				return objectField;
			}
			else if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
			{
				var container = new Foldout { text = type.Name, value = true };
				foreach (var fieldInfo in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
				{
					var fieldValue = fieldInfo.GetValue(value);
					
					var fieldElement = CreateInputFieldForType(fieldInfo.FieldType, fieldValue, newValue =>
					{
						fieldInfo.SetValue(value, newValue);
						setValue(value);
					}, isForField);
					if (isForField) fieldElement.style.marginRight = 9;

					container.Add(new Label(fieldInfo.Name) { style = { unityFontStyleAndWeight = FontStyle.Bold } });
					container.Add(fieldElement);
				}
				return container;
			}

			return new Label($"Unsupported type: {type.Name}");
		}

		VisualElement CreateSection(string label)
		{
			var sectionContainer = new VisualElement();
			sectionContainer.style.marginTop = 20;

			var titleLabel = new Label(label);
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.fontSize = 12;
			titleLabel.style.color = new StyleColor(Color.white);
			titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

			sectionContainer.Add(titleLabel);

			var divider = new VisualElement();
			divider.style.height = 1;
			divider.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f, 1f));
			divider.style.marginTop = 7;
			divider.style.marginBottom = 6;

			sectionContainer.Add(divider);

			return sectionContainer;
		}
	}
}
